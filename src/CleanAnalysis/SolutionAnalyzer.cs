using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace CleanAnalysis
{
    public class SolutionAnalyzer
    {
        private HashSet<CrossAssemblyReference> CrossAssemblyReferences { get; }
            = new HashSet<CrossAssemblyReference>();
        private HashSet<string> SolutionAssemblyNames { get; } = new HashSet<string>();
        private Dictionary<Project, string> AssemblyNames { get; } = new Dictionary<Project, string>();

        public async Task<Dictionary<Project, Metrics>> AnalyzeSolution(Solution solution)
        {
            var projects = FilterOutTestingProjects(solution.Projects.ToList());
            foreach (var project in projects)
            {
                SolutionAssemblyNames.Add(project.AssemblyName);
                AssemblyNames[project] = project.AssemblyName;
            }
            var projectMetrics = new Dictionary<Project, Metrics>();
            foreach (var project in projects)
            {
                projectMetrics[project] = await AnalyzeProject(project);
            }
            projectMetrics = CalculateStability(projectMetrics);
            return projectMetrics;
        }

        private IList<Project> FilterOutTestingProjects(IList<Project> projects)
            => projects.Where(p => !p.Name.Contains("Tests")).ToList();

        private Dictionary<Project, Metrics> CalculateStability(Dictionary<Project, Metrics> projectMetrics)
        {
            foreach (var project in projectMetrics.Keys.ToList())
            {
                var targetAssemblyName = AssemblyNames[project];
                var dependentCount = CrossAssemblyReferences
                    .Where(xref => xref.TargetAssembly == targetAssemblyName)
                    .Select(xref => xref.OriginType)
                    .Distinct()
                    .Count();
                var metrics = projectMetrics[project];
                projectMetrics[project] = new Metrics(
                    new StabilityMetric(metrics.Stability.Dependencies, dependentCount),
                    metrics.Abstractness);
            }
            return projectMetrics;
        }

        public async Task<Metrics> AnalyzeProject(Project project)
        {
            var compilation = await project.GetCompilationAsync();
            var abstractness = CalculateAbstractness(compilation);
            var stability = PrepareStabilityMetric(compilation);
            return new Metrics(stability, abstractness);
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
    }
}
