namespace CleanAnalysis
{
    public sealed class SolutionAnalysisDiagnostic
    {
        private SolutionAnalysisDiagnostic(string message)
        {
            Message = message;
        }

        public string Message { get; }

        public static implicit operator SolutionAnalysisDiagnostic(string message)
        {
            return new SolutionAnalysisDiagnostic(message);
        }
    }
}
