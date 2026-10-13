using System.Text;

namespace AIHelper.Shared
{
    /// <summary>
    /// Puts one argument back into the single string a Windows process is started with.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the C# half of a format shared with aihelper-launch-assistant. AI Helper writes the
    /// command line the assistant is started with; the assistant takes that line apart and writes the
    /// command lines the game and the restarted AI Helper are started with. Every one of those
    /// programs is a writer and a reader of the same format, and it is only correct if all of them
    /// agree on it.
    /// </para>
    /// <para>
    /// The other half is AIHelperLaunchAssistant\ArgumentQuoting.cpp, which applies the same rules in
    /// the language the assistant is written in. The two cannot be one file, so they are kept in step
    /// by hand and held together by the end-to-end harness: it starts the real assistant with a line
    /// built here and reads back what the programs that assistant starts actually received.
    /// </para>
    /// <para>
    /// The rules below are the ones CommandLineToArgvW - which is what each of those programs uses to
    /// take a line apart again - actually applies, and they are the reason this cannot simply be
    /// "\"" + value + "\"": a path ending in a backslash needs that backslash doubled, and a value
    /// containing a quote needs it escaped with backslashes of its own.
    /// </para>
    /// </remarks>
    internal static class ArgumentQuoting
    {
        /// <summary>
        /// Quotes <paramref name="argument"/> if it needs it, and returns it unchanged if it does not.
        /// </summary>
        internal static string Quote(string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                return "\"\"";
            }

            if (argument.IndexOfAny(CharactersNeedingQuotes) < 0)
            {
                return argument;
            }

            var builder = new StringBuilder(argument.Length + 2);
            builder.Append('"');

            int backslashes = 0;
            foreach (char character in argument)
            {
                if (character == '\\')
                {
                    // A run of backslashes is only doubled in front of a quote, so it is counted here
                    // and written out when the character after it is known.
                    backslashes++;
                    continue;
                }

                if (character == '"')
                {
                    // A quote is escaped by the backslashes before it, doubled, plus the escape itself.
                    builder.Append('\\', (backslashes * 2) + 1);
                    builder.Append('"');
                    backslashes = 0;
                    continue;
                }

                if (backslashes > 0)
                {
                    builder.Append('\\', backslashes);
                    backslashes = 0;
                }

                builder.Append(character);
            }

            // A run of backslashes just before the closing quote must be doubled as well.
            builder.Append('\\', backslashes * 2);
            builder.Append('"');

            return builder.ToString();
        }

        /// <summary>
        /// Joins arguments into one command line, quoting each of them so that the next process takes
        /// the line apart into exactly the same list.
        /// </summary>
        internal static string Join(string[] arguments)
        {
            if (arguments == null || arguments.Length == 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            for (int index = 0; index < arguments.Length; index++)
            {
                if (index > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(Quote(arguments[index]));
            }

            return builder.ToString();
        }

        private static readonly char[] CharactersNeedingQuotes = { ' ', '\t', '"' };
    }
}
