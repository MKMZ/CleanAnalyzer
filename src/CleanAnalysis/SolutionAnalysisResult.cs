using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace CleanAnalysis
{
    public struct SolutionAnalysisResult
    {
        public SolutionAnalysisResult(ImmutableDictionary<Project, Metrics> projectMetrics, ImmutableArray<PackagingPrincipleDiagnostic> diagnostics)
        {
            ProjectMetrics = projectMetrics;
            Diagnostics = diagnostics;
        }

        public ImmutableDictionary<Project, Metrics> ProjectMetrics { get; }

        public ImmutableArray<PackagingPrincipleDiagnostic> Diagnostics { get; }
    }
}
