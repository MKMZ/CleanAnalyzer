//using System;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.CodeAnalysis;
//using MoreLinq;

//namespace CleanAnalysis
//{
//    public class SolutionAnalyzer
//    {
//        public SolutionAnalyzer(Solution solution, double mainSequenceDistanceAllowance = 0.7)
//        {
//            Solution = solution;
//            MainSequenceDistanceAllowance = mainSequenceDistanceAllowance;
//        }

//        private HashSet<CrossAssemblyReference> CrossAssemblyReferences { get; }
//            = new HashSet<CrossAssemblyReference>();

//        private HashSet<string> SolutionAssemblyNames { get; } = new HashSet<string>();

//        public Solution Solution { get; }

//        public double MainSequenceDistanceAllowance { get; }

//        public async Task<SolutionAnalysisResult> AnalyzeSolution(
//            IProgress<SolutionAnalysisDiagnostic> progress = null)
//        {
//            progress?.Report("Analysis started");
//            progress?.Report("Creating analyzed projects list");
//            var projects = FilterOutTestingProjects(Solution.Projects.ToList());
//            foreach (var project in projects)
//            {
//                SolutionAssemblyNames.Add(project.AssemblyName);
//            }
//            var projectMetrics = new Dictionary<Project, Metrics>();
//            foreach (var project in projects)
//            {
//                progress?.Report($"Calculating metrics for project {project.Name}");
//                projectMetrics[project] = await AnalyzeProjectMetrics(project);
//            }
//            foreach (var project in projects)
//            {
//                progress?.Report($"Updating stability metric for project {project.Name}");
//                projectMetrics[project] = UpdateStabilityDependents(
//                    project.AssemblyName,
//                    projectMetrics[project]);
//            }
//            progress?.Report("Generating diagnostics");
//            var diagnostics = CreateDiagnostics(projectMetrics.ToImmutableDictionary());
//            progress?.Report("Analysis finished");
//            return new SolutionAnalysisResult(
//                projectMetrics.ToImmutableDictionary(),
//                diagnostics);
//        }

//        private async Task<Metrics> AnalyzeProjectMetrics(Project project)
//        {
//            var compilation = await project.GetCompilationAsync();
//            SearchForCrossReferences(compilation);
//            return await CalculateProjectMetrics(project);
//        }

//        private async Task<Metrics> CalculateProjectMetrics(Compilation compilation)
//        {
//            var abstractness = CalculateAbstractness(compilation);
//            var stability = CalculateStability(compilation);
//            return new Metrics(stability, abstractness);
//        }

//        private ImmutableArray<Project> FilterOutTestingProjects(IList<Project> projects)
//            => projects.Where(p => !p.Name.Contains("Tests")).ToImmutableArray();

//        private Metrics UpdateStabilityDependents(string targetAssemblyName, Metrics metrics)
//        {
//            var dependentCount = CrossAssemblyReferences
//                .Where(xref => xref.TargetAssembly == targetAssemblyName)
//                .DistinctBy(xref => (xref.OriginType, xref.OriginAssembly))
//                .Count();
//            return new Metrics(
//                new StabilityMetric(metrics.Stability.Dependencies, dependentCount),
//                metrics.Abstractness);
//        }

//        private AbstractnessMetric CalculateAbstractness(Compilation compilation)
//        {
//            var visitor = new AbstractnessVisitor();
//            visitor.Visit(compilation.Assembly);
//            return new AbstractnessMetric(
//                visitor.Abstractions.Count,
//                visitor.Concretizations.Count);
//        }

//        private void SearchForCrossReferences(Compilation compilation)
//        {
//            var visitor = new StabilityVisitor(compilation.Assembly, Solution);
//            visitor.Visit(compilation.Assembly);
//            CrossAssemblyReferences.UnionWith(visitor.CrossReferences);
//        }

//        private CrossAssemblyReference CreateCrossAssemblyReference(INamedTypeSymbol origin, INamedTypeSymbol target)
//                => new CrossAssemblyReference(
//                    target.ContainingAssembly.MetadataName,
//                    target.GetFullName(),
//                    origin.ContainingAssembly.MetadataName,
//                    origin.GetFullName());

//        private ImmutableArray<PackagingPrincipleDiagnostic> CreateDiagnostics(
//            ImmutableDictionary<Project, Metrics> projectMetrics)
//        {
//            return Inner().ToImmutableArray();

//            IEnumerable<PackagingPrincipleDiagnostic> Inner()
//            {
//                foreach (var project in projectMetrics.Keys)
//                {
//                    var metric = projectMetrics[project];
//                    var abstractness = metric.Abstractness.Coefficient;
//                    var stability = metric.Stability.Coefficient;
//                    foreach (var dependencyRef in project.ProjectReferences)
//                    {
//                        var dependencyProject = project.Solution.GetProject(dependencyRef.ProjectId);
//                        var dependencyMetric = projectMetrics[dependencyProject];
//                        var dependencyStability = dependencyMetric.Stability.Coefficient;
//                        if (stability > dependencyStability)
//                        {
//                            yield return PackagingPrincipleDiagnostic.Create(
//                                "PPD01",
//                                "Project '{0}' depends on a less stable project '{1}'. ({2}->{3})",
//                                project.Name,
//                                dependencyProject.Name,
//                                stability,
//                                dependencyStability);
//                        }
//                    }
//                    if (metric.MainSequenceDistance > MainSequenceDistanceAllowance)
//                    {
//                        if (abstractness > stability)
//                        {
//                            // useless zone
//                            yield return PackagingPrincipleDiagnostic.Create(
//                                "PPD02",
//                                "Project '{0}' belongs to the useless zone (more abstract than stable). MSD={1}",
//                                project.Name,
//                                metric.MainSequenceDistance);
//                        }
//                        else
//                        {
//                            // pain zone
//                            yield return PackagingPrincipleDiagnostic.Create(
//                                "PPD03",
//                                "Project '{0}' belongs to the pain zone (more stable than abstract). MSD={1}",
//                                project.Name,
//                                metric.MainSequenceDistance);
//                        }
//                    }
//                }
//            }
//        }
//    }
//}
