namespace CleanAnalysis
{
    public sealed class PackagingPrincipleDiagnostic
    {
        private PackagingPrincipleDiagnostic(string code, string messageFormat, object[] args)
        {
            MessageFormat = messageFormat;
            Args = args;
            Code = code;
        }

        public string Code { get; }

        public string Message => string.Format(MessageFormat, Args);

        private object[] Args { get; }
        private string MessageFormat { get; }

        public static PackagingPrincipleDiagnostic Create(string code, string messageFormat, params object[] args)
        {
            return new PackagingPrincipleDiagnostic(code, messageFormat, args);
        }

        public override string ToString() => $"{Code}: {Message}";
    }
}
