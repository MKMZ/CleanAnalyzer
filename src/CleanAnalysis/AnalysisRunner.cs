using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace CleanAnalysis
{
    class AnalysisRunner
    {
        private IConsoleProgressReporter ProgressReporter { get; }
        private ICalculator<AbstractnessMetric> AbstractnessCalculator { get; }
        private ICalculator<StabilityMetric> StabilityCalculator { get; }
        private DataGatherer DataGatherer { get; }

        public AnalysisRunner(
            IConsoleProgressReporter progressReporter, 
            DataGatherer dataGatherer, 
            ICalculator<AbstractnessMetric> abstractnessCalculator,
            ICalculator<StabilityMetric> stabilityCalculator)
        {
            ProgressReporter = progressReporter;
            DataGatherer = dataGatherer;
            AbstractnessCalculator = abstractnessCalculator;
            StabilityCalculator = stabilityCalculator;
        }

        public async Task<SolutionAnalysisResult> Run(string[] solutionPaths, double mainSequenceDistanceAllowance = 0.7)
        {
            var analysisDataContainer = await DataGatherer.Gather(solutionPaths);

            var abstractnessMetrics = AbstractnessCalculator.Calculate(analysisDataContainer);
            var stabilityMetrics = StabilityCalculator.Calculate(analysisDataContainer);
            var projectMetrics = CreateMetrics(abstractnessMetrics, stabilityMetrics);
            var diagnostics = CreateDiagnostics(projectMetrics, mainSequenceDistanceAllowance);
            return new SolutionAnalysisResult(projectMetrics, diagnostics);
        }

        private static IImmutableList<ProjectMetrics> CreateMetrics(IImmutableDictionary<Project, AbstractnessMetric> abstractnessMetrics, IImmutableDictionary<Project, StabilityMetric> stabilityMetrics) 
            => abstractnessMetrics
                .Keys
                .Select(project => new ProjectMetrics(project, new Metrics(stabilityMetrics[project], abstractnessMetrics[project])))
                .ToImmutableList();

        private IImmutableList<PackagingPrincipleDiagnostic> CreateDiagnostics(
            IImmutableList<ProjectMetrics> projectMetrics,
            double mainSequenceDistanceAllowance)
        {
            return Inner().ToImmutableList();

            IEnumerable<PackagingPrincipleDiagnostic> Inner()
            {
                foreach (var projectMetric in projectMetrics)
                {
                    var abstractness = projectMetric.Metrics.Abstractness.Coefficient;
                    var stability = projectMetric.Metrics.Stability.Coefficient;
                    foreach (var dependencyRef in projectMetric.Project.ProjectReferences)
                    {
                        var dependencyProject = projectMetric.Project.Solution.GetProject(dependencyRef.ProjectId);
                        var dependencyMetric = projectMetrics.First(p => p.Project.Name == dependencyProject.Name);
                        var dependencyStability = dependencyMetric.Metrics.Stability.Coefficient;
                        if (stability > dependencyStability)
                        {
                            yield return PackagingPrincipleDiagnostic.Create(
                                "PPD01",
                                "Project '{0}' depends on a less stable project '{1}'. ({2}->{3})",
                                projectMetric.Project.Name,
                                dependencyProject.Name,
                                stability,
                                dependencyStability);
                        }
                    }
                    if (projectMetric.Metrics.MainSequenceDistance > mainSequenceDistanceAllowance)
                    {
                        if (abstractness > stability)
                        {
                            // useless zone
                            yield return PackagingPrincipleDiagnostic.Create(
                                "PPD02",
                                "Project '{0}' belongs to the useless zone (more abstract than stable). MSD={1}",
                                projectMetric.Project.Name,
                                projectMetric.Metrics.MainSequenceDistance);
                        }
                        else
                        {
                            // pain zone
                            yield return PackagingPrincipleDiagnostic.Create(
                                "PPD03",
                                "Project '{0}' belongs to the pain zone (more stable than abstract). MSD={1}",
                                projectMetric.Project.Name,
                                projectMetric.Metrics.MainSequenceDistance);
                        }
                    }
                }
            }
        }
    }
}
