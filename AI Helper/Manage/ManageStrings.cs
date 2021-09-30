using System;
using System.Collections.Generic;
using System.Globalization;

namespace AIHelper.Manage
{
    static class ManageStrings
    {
        /// <summary>
        /// check if path is long and can cause standart io operations errors.
        /// if path is long, will be added prefix "\\?\" for long paths.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static bool CheckForLongPath(ref string path)
        {
            if (path.Length > 259 && path.Substring(0, 4) != @"\\?\")
            {
                ManageLogs.Log("Warning. Path to file has more of 259 characters. It can cause errors in game. Try to make path shorter by rename filename or any folders to it. File:" + Environment.NewLine + path);
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
        /// <param name="input"></param>
        /// <returns></returns>
        internal static bool IsMultiline(this string input)
        {
            //inputString="line1\r\n"
            //00.00066x100000
            //var count = 0;
            //var index = -1;

            //do
            //{
            //    if (++count > 1)
            //    {
            //        return true;
            //    }

            //    index = inputString.IndexOf('\n', index + 1);
            //}
            //while (index != -1);

            //return false;

            //inputString="line1\r\n"
            //00.00055x100000
            return input.IndexOf('\n', 0) > -1;

            //inputString="line1\r\n"
            //00.0021x100000
            //return inputString.Contains("\n");

            ///old
            //{
            //0.0035x100000
            //if (input != null)
            //{
            //    using (System.IO.StringReader reader = new System.IO.StringReader(input))
            //    {
            //        int i = 0;
            //        while (reader.ReadLine() != null)
            //        {
            //            i++;
            //            if (i > 1)
            //            {
            //                return true;
            //            }
            //        }
            //    }
            //}

            //return false;
            //}
        }

        internal static string TrimFileVersion(this string fileVersion)
        {
            return fileVersion.Trim().TrimStart('v', 'V', 'r', 'R', '.', ',').TrimEnd('.', ',', '0');
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
            if (name.StartsWith("[" + ManageSettings.GetCurrentGame().GetGamePrefix() + "][", System.StringComparison.InvariantCulture) || (name.StartsWith("[", System.StringComparison.InvariantCulture) && !name.StartsWith("[" + ManageSettings.GetCurrentGame().GetGamePrefix() + "]", System.StringComparison.InvariantCulture)) || ManageStrings.IsStringAContainsStringB(name, author))
            {
            }
            else if (author.Length > 0)
            {
                //проверка на любые невалидные для имени папки символы
                if (ManageFilesFoldersExtensions.ContainsAnyInvalidCharacters(author))
                {
                }
                else
                {
                    name = "[" + author + "] " + name;
                }
            }

            return name;
        }

        public static bool IsStringAContainsAnyStringFromStringArray(string stringA, string[] stringArray, bool ignoreCase = false)
        {
            foreach (var stringB in stringArray)
            {
                if (ignoreCase)
                {
                    if (IsStringAContainsStringB(stringA, stringB) || IsStringAContainsStringB(stringA.ToUpperInvariant(), stringB.ToUpperInvariant()))
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
