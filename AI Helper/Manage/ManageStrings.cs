using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace AIHelper.Manage
{
    static class ManageStrings
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// check if path is long and can cause standart io operations errors.
        /// if path is long, will be added prefix "\\?\" for long paths.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>true when path was changed to long path format</returns>
        internal static bool CheckForLongPath(ref string path)
        {
            if (path.Length > ManageStringsExtensions.MaxPathLengthForDirectory
                && !path.StartsWith(ManageStringsExtensions.LongPathPrefix, StringComparison.Ordinal))
            {
                _log.Warn("Warning. Path has more of " + ManageStringsExtensions.MaxPathLengthForDirectory
                    + " characters. It can cause errors in game. Try to make path shorter by rename filename or any folders to it. File:"
                    + Environment.NewLine + path);
                path = path.ToLongPathWhenNeed(true, true);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Split string to lines
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static IEnumerable<string> SplitToLines(this string input)
        {
            //https://stackoverflow.com/a/23408020
            if (input == null)
            {
                yield break;
            }

            if (!input.IsMultiline())
            {
                yield return input;
                yield break;
            }

            //Fix of last newline \n symbol was not returned 
            bool endsWithNewLine = input.EndsWith("\n", System.StringComparison.InvariantCulture);

            using (System.IO.StringReader reader = new System.IO.StringReader(input))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
                if (endsWithNewLine)//if string endswith \n then last line will be null
                {
                    yield return string.Empty;
                }
            }
        }

        /// <summary>
        /// If string is multiline
        /// </summary>
        /// <param name="input">The text to test. Null counts as a single line.</param>
        /// <returns>true when the text holds at least one line break.</returns>
        internal static bool IsMultiline(this string input)
        {
            // A null or empty value has no line break in it. SplitToLines accepts a null
            // input and then hands it here, so this has to as well.
            if (string.IsNullOrEmpty(input)) return false;

            return input.IndexOf('\n') > -1;
        }

        internal static string TrimFileVersion(this string fileVersion)
        {
            fileVersion = fileVersion.Trim().TrimStart('v', 'V', 'r', 'R', '_', '.', ',').TrimEnd('.', ',');

            if (fileVersion.Length == 0)
            {
                return fileVersion;
            }

            while (fileVersion.EndsWith(".0") || fileVersion.EndsWith(",0"))
            {
                fileVersion = fileVersion.Remove(fileVersion.Length - 2, 2);
            }
            while (fileVersion.StartsWith(".0") || fileVersion.StartsWith(",0"))
            {
                fileVersion = fileVersion.Remove(0, 2);
            }

            return fileVersion;
        }

        /// <summary>
        /// Check if string A contains string B (if length of A > length of A with replaced B by "")<br></br><br></br>
        /// Speed check tests: https://cc.davelozinski.com/c-sharp/fastest-way-to-check-if-a-string-occurs-within-a-string
        /// </summary>
        /// <param name="stringAWhereSearch"></param>
        /// <param name="stringBToSearch"></param>
        /// <returns></returns>
        public static bool IsStringAContainsStringB(string stringAWhereSearch, string stringBToSearch)
        {
            return stringAWhereSearch.IsContains(stringBToSearch);
        }

        public static bool IsStringAequalsStringB(string stringA, string stringB, bool ignoreCase = false)
        {
            if (ignoreCase)
            {
                return string.Compare(stringA, stringB, ignoreCase, CultureInfo.InvariantCulture) == 0;
            }
            else
            {
                return string.CompareOrdinal(stringA, stringB) == 0;
            }
        }

        public static string AddAuthorToNameIfNeed(string name, string author)
        {
            //добавление имени автора в начало имени папки
            bool nameAlreadyHasAuthorOrGameMark =
                name.StartsWith("[" + ManageSettings.CurrentGame.GameAbbreviation + "][", StringComparison.InvariantCulture)
                || (name.StartsWith("[", StringComparison.InvariantCulture) && !name.StartsWith("[" + ManageSettings.CurrentGame.GameAbbreviation + "]", StringComparison.InvariantCulture))
                || ManageStrings.IsStringAContainsStringB(name, author);

            //проверка на любые невалидные для имени папки символы
            bool authorIsValid = author.Length > 0 && !ManageFilesFoldersExtensions.ContainsAnyInvalidCharacters(author);

            return nameAlreadyHasAuthorOrGameMark || !authorIsValid
                ? name
                : "[" + author + "] " + name;
        }

        public static bool IsStringAContainsAnyStringFromStringArray(string stringA, IEnumerable<string> stringArray, bool ignoreCase = false)
        {
            foreach (var stringB in stringArray)
            {
                if (ignoreCase)
                {
                    if (IsStringAContainsStringB(stringA.ToUpperInvariant(), stringB.ToUpperInvariant()))
                    {
                        return true;
                    }
                }
                else
                {
                    if (IsStringAContainsStringB(stringA, stringB))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
