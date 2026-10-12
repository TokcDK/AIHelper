namespace AIHelperLaunchAssistant
{
    /// <summary>What a raw command line turned out to be.</summary>
    internal enum ParseStatus
    {
        /// <summary>A launch request was read.</summary>
        Success,

        /// <summary>The command line asked for the usage text.</summary>
        HelpRequested,

        /// <summary>The command line asked for the version.</summary>
        VersionRequested,

        /// <summary>The command line could not be read; see <see cref="ParseResult.Message"/>.</summary>
        Error
    }

    /// <summary>The outcome of reading a command line.</summary>
    internal sealed class ParseResult
    {
        private ParseResult(ParseStatus status, LaunchOptions options, string message)
        {
            Status = status;
            Options = options;
            Message = message;
        }

        internal ParseStatus Status { get; }

        /// <summary>The launch request; only set when <see cref="Status"/> is <see cref="ParseStatus.Success"/>.</summary>
        internal LaunchOptions Options { get; }

        /// <summary>Why the command line could not be read; only set for <see cref="ParseStatus.Error"/>.</summary>
        internal string Message { get; }

        internal bool IsSuccess
        {
            get { return Status == ParseStatus.Success; }
        }

        internal static ParseResult Success(LaunchOptions options)
        {
            return new ParseResult(ParseStatus.Success, options, null);
        }

        internal static ParseResult Help()
        {
            return new ParseResult(ParseStatus.HelpRequested, null, null);
        }

        internal static ParseResult Version()
        {
            return new ParseResult(ParseStatus.VersionRequested, null, null);
        }

        internal static ParseResult Error(string message)
        {
            return new ParseResult(ParseStatus.Error, null, message);
        }
    }
}
