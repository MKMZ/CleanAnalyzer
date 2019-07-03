using System;
using Microsoft.CodeAnalysis.MSBuild;

namespace CleanAnalysis
{
    public interface IConsoleProgressReporter
        : IProgress<ProjectLoadProgress>, IProgress<SolutionAnalysisDiagnostic>
    {
    }
}
