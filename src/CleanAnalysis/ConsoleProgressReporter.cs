using System;
using System.IO;
using Microsoft.CodeAnalysis.MSBuild;

namespace CleanAnalysis
{
    public class ConsoleProgressReporter
        : IConsoleProgressReporter
    {
        public void Report(ProjectLoadProgress loadProgress)
        {
            var projectDisplay = Path.GetFileName(loadProgress.FilePath);
            if (loadProgress.TargetFramework != null)
            {
                projectDisplay += $" ({loadProgress.TargetFramework})";
            }

            Console.WriteLine($"{loadProgress.Operation,-15} {loadProgress.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
        }

        public void Report(SolutionAnalysisDiagnostic value)
        {
            Console.WriteLine($"  {value.Message}");
        }
    }
}
