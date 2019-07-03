using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace CleanAnalysis
{
    public struct SolutionAnalysisResult
    {
        public SolutionAnalysisResult(IImmutableList<ProjectMetrics> projectMetrics, IImmutableList<PackagingPrincipleDiagnostic> diagnostics)
        {
            ProjectMetrics = projectMetrics;
            Diagnostics = diagnostics;
        }

        public IImmutableList<ProjectMetrics> ProjectMetrics { get; }

        public IImmutableList<PackagingPrincipleDiagnostic> Diagnostics { get; }
    }
}
