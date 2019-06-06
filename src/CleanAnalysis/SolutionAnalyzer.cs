using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MoreLinq;

namespace CleanAnalysis
{
    public class SolutionAnalyzer
    {
        public SolutionAnalyzer(Solution solution, double mainSequenceDistanceAllowance = 0.7)
        {
            Solution = solution;
            MainSequenceDistanceAllowance = mainSequenceDistanceAllowance;
        }

        private HashSet<CrossAssemblyReference> CrossAssemblyReferences { get; }
            = new HashSet<CrossAssemblyReference>();

        private HashSet<string> SolutionAssemblyNames { get; } = new HashSet<string>();

        public Solution Solution { get; }

        public double MainSequenceDistanceAllowance { get; }

        public async Task<SolutionAnalysisResult> AnalyzeSolution()
        {
            var projects = FilterOutTestingProjects(Solution.Projects.ToList());
            foreach (var project in projects)
            {
                SolutionAssemblyNames.Add(project.AssemblyName);
            }
            var projectMetrics = new Dictionary<Project, Metrics>();
            foreach (var project in projects)
            {
                projectMetrics[project] = await CalculateProjectMetrics(project);
            }
            foreach (var project in projects)
            {
                projectMetrics[project] = UpdateStabilityDependents(
                    project.AssemblyName,
                    projectMetrics[project]);
            }
            var projectMetrics1 = projectMetrics.ToImmutableDictionary();
            var diagnostics = CreateDiagnostics(projectMetrics1);
            return new SolutionAnalysisResult(
                projectMetrics1,
                diagnostics);
        }

        private async Task<Metrics> CalculateProjectMetrics(Project project)
        {
            var compilation = await project.GetCompilationAsync();
            var abstractness = CalculateAbstractness(compilation);
            var stability = PrepareStabilityMetric(compilation);
            return new Metrics(stability, abstractness);
        }

        private ImmutableArray<Project> FilterOutTestingProjects(IList<Project> projects)
            => projects.Where(p => !p.Name.Contains("Tests")).ToImmutableArray();

        private Metrics UpdateStabilityDependents(string targetAssemblyName, Metrics metrics)
        {
            var dependentCount = CrossAssemblyReferences
                .Where(xref => xref.TargetAssembly == targetAssemblyName)
                .DistinctBy(xref => (xref.OriginType, xref.OriginAssembly))
                .Count();
            return new Metrics(
                new StabilityMetric(metrics.Stability.Dependencies, dependentCount),
                metrics.Abstractness);
        }

        private AbstractnessMetric CalculateAbstractness(Compilation compilation)
        {
            var visitor = new AbstractnessVisitor();
            visitor.Visit(compilation.Assembly);
            return new AbstractnessMetric(
                visitor.Abstractions.Count,
                visitor.Concretizations.Count);
        }

        private StabilityMetric PrepareStabilityMetric(Compilation compilation)
        {
            var visitor = new StabilityVisitor(compilation.Assembly, SolutionAssemblyNames);
            visitor.Visit(compilation.Assembly);
            var xAssemblyRefs = visitor.ExternalTypeReferencingTypes
                .SelectMany(pair => pair.Value.Select(origin => CreateCrossAssemblyReference(origin, pair.Key)));
            foreach (var xRef in xAssemblyRefs)
            {
                CrossAssemblyReferences.Add(xRef);
            }
            return new StabilityMetric(
                visitor.ExternalTypesUsed.Count,
                default);
        }

        private CrossAssemblyReference CreateCrossAssemblyReference(INamedTypeSymbol origin, INamedTypeSymbol target)
                => new CrossAssemblyReference(
                    target.ContainingAssembly.MetadataName,
                    target.GetFullName(),
                    origin.ContainingAssembly.MetadataName,
                    origin.GetFullName());

        private ImmutableArray<PackagingPrincipleDiagnostic> CreateDiagnostics(
            ImmutableDictionary<Project, Metrics> projectMetrics)
        {
            return Inner().ToImmutableArray();

            IEnumerable<PackagingPrincipleDiagnostic> Inner()
            {
                foreach (var project in projectMetrics.Keys)
                {
                    var metric = projectMetrics[project];
                    var abstractness = metric.Abstractness.Coefficient;
                    var stability = metric.Stability.Coefficient;
                    foreach (var dependencyRef in project.ProjectReferences)
                    {
                        var dependencyProject = project.Solution.GetProject(dependencyRef.ProjectId);
                        var dependencyMetric = projectMetrics[dependencyProject];
                        var dependencyStability = dependencyMetric.Stability.Coefficient;
                        if (stability > dependencyStability)
                        {
                            yield return PackagingPrincipleDiagnostic.Create(
                                "PPD01",
                                "Project '{0}' depends on a less stable project '{1}'. ({2}->{3})",
                                project.Name,
                                dependencyProject.Name,
                                stability,
                                dependencyStability);
                        }
                    }
                    if (metric.MainSequenceDistance > MainSequenceDistanceAllowance)
                    {
                        if (abstractness > stability)
                        {
                            // useless zone
                            yield return PackagingPrincipleDiagnostic.Create(
                                "PPD02",
                                "Project '{0}' belongs to the useless zone (more abstract than stable). MSD={1}",
                                project.Name,
                                metric.MainSequenceDistance);
                        }
                        else
                        {
                            // pain zone
                            yield return PackagingPrincipleDiagnostic.Create(
                                "PPD03",
                                "Project '{0}' belongs to the pain zone (more stable than abstract). MSD={1}",
                                project.Name,
                                metric.MainSequenceDistance);
                        }
                    }
                }
            }
        }
    }
}
