using System;
using System.Collections.Generic;
using System.IO;

namespace AIHelperUpdater
{
    /// <summary>
    /// Reads update-rules.txt, the list of what an update backs up and what it leaves alone.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The format is deliberately the smallest one that does the job: one path per line, relative
    /// to the application folder, blank lines and lines starting with <c>#</c> ignored, and a
    /// leading <c>!</c> marking a path the backup must not move. Nothing is escaped and no path
    /// is interpreted - a Windows path may contain spaces, dots, brackets and non-ASCII text, and
    /// every one of them is just a character here.
    /// </para>
    /// <para>
    /// Reading is separated from acting on the paths: this type only turns text into rules, and
    /// whether a path is inside the application folder, or reserved by the update itself, is
    /// decided where the folder is known.
    /// </para>
    /// </remarks>
    internal sealed class UpdateRulesFile
    {
        private const char CommentPrefix = '#';
        private const char ExclusionPrefix = '!';

        internal UpdateRulesFile(string filePath)
        {
            FilePath = filePath;
        }

        /// <summary>File the rules are read from.</summary>
        internal string FilePath { get; }

        /// <summary>
        /// Reads the rules.
        /// </summary>
        /// <remarks>
        /// A missing or unreadable file is reported rather than treated as "no rules". An empty
        /// rule list would mean "back nothing up", which is a silent way to lose the old version;
        /// the caller has to be able to refuse the update instead.
        /// </remarks>
        /// <param name="error">Why the rules could not be read, or null.</param>
        /// <returns>The rules, or null when they could not be read.</returns>
        internal IReadOnlyList<UpdateRule> TryRead(out string error)
        {
            error = null;

            string[] lines;
            try
            {
                lines = File.ReadAllLines(FilePath);
            }
            catch (Exception ex)
            {
                error = "the rules file '" + FilePath + "' could not be read: " + ex.Message;
                return null;
            }

            var rules = new List<UpdateRule>(lines.Length);

            foreach (var line in lines)
            {
                var text = line.Trim();
                if (text.Length == 0 || text[0] == CommentPrefix) continue;

                var isExclusion = text[0] == ExclusionPrefix;
                if (isExclusion) text = text.Substring(1).Trim();

                // "!" on its own is a line that says nothing.
                if (text.Length == 0) continue;

                rules.Add(new UpdateRule(text, isExclusion));
            }

            return rules;
        }
    }
}
