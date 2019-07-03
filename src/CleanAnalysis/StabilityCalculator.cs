using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CleanAnalysis
{
    internal class StabilityCalculator
        : ICalculator<StabilityMetric>
    {
        public IImmutableDictionary<Project, StabilityMetric> Calculate(AnalysisDataContainer analysisDataContainer)
        {
            var crossAssemblyReferences = CheckReferencesToAllProjects(analysisDataContainer);
            return CountMetric(crossAssemblyReferences, analysisDataContainer);
        }

        private IImmutableDictionary<Project, StabilityMetric> CountMetric(ISet<CrossAssemblyReference> references, AnalysisDataContainer analysisDataContainer)
        {
            var dictionary = analysisDataContainer
                .Projects
                .ToDictionary(project => project.Compilation.Assembly.Name, project => new StabilityMetric());
            foreach (var reference in references)
            {
                dictionary[reference.OriginAssembly] = dictionary[reference.OriginAssembly].IncreaseDependencies();
                dictionary[reference.TargetAssembly] = dictionary[reference.TargetAssembly].IncreaseDependents();
            }
            return ConcertMetricDictionaryKeysToProject(dictionary.ToImmutableDictionary(), analysisDataContainer.Projects);
        }

        private IImmutableDictionary<Project, StabilityMetric> ConcertMetricDictionaryKeysToProject(
            IImmutableDictionary<string, StabilityMetric> inputDictionary,
            IImmutableList<ProjectData> projects) 
            => projects
                .Select(project => new KeyValuePair<Project, StabilityMetric>(project.Info, inputDictionary[project.Compilation.Assembly.Name]))
                .ToImmutableDictionary();

        private ISet<CrossAssemblyReference> CheckReferencesToAllProjects(AnalysisDataContainer analysisDataContainer)
            => analysisDataContainer
                .Projects
                .Select(project => CheckReferencesToProject(project, analysisDataContainer.Solutions))
                .Aggregate(new HashSet<CrossAssemblyReference>(), (acc, set) => acc.Concat(set).ToHashSet());

        private ISet<CrossAssemblyReference> CheckReferencesToProject(ProjectData projectData, IImmutableList<Solution> solutions)
        {
            var stabilityVisitor = new StabilityVisitor(projectData.Compilation.Assembly, solutions);
            stabilityVisitor.StartTour();
            return stabilityVisitor.CrossReferences;
        }
    }
}
