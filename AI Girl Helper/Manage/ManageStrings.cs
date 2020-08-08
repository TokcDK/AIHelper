using System;
using System.Collections.Generic;
using System.Globalization;

namespace AIHelper.Manage
{
    static class ManageStrings
    {
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
            bool EndsWithNewLine = input.EndsWith("\n", System.StringComparison.InvariantCulture);

            using (System.IO.StringReader reader = new System.IO.StringReader(input))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
                if (EndsWithNewLine)//if string endswith \n then last line will be null
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
            {
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
            }
        }

        internal static string TrimFileVersion(this string fileVersion)
        {
            return fileVersion.Trim().TrimStart('v','r').TrimEnd('.', '0');
        }

        /// <summary>
        /// Check if string A contains string B (if length of A > length of A with replaced B by "")<br></br><br></br>
        /// Speed check tests: https://cc.davelozinski.com/c-sharp/fastest-way-to-check-if-a-string-occurs-within-a-string
        /// </summary>
        /// <param name="StringAWhereSearch"></param>
        /// <param name="StringBToSearch"></param>
        /// <returns></returns>
        public static bool IsStringAContainsStringB(string StringAWhereSearch, string StringBToSearch)
        {
            int StringAInWhichSearchLength = StringAWhereSearch.Length;
            if (StringAInWhichSearchLength > 0 && StringBToSearch.Length > 0)//safe check for empty values
            {
                //if string A contains string B then string A with replaced stringB by empty will be
                return StringAInWhichSearchLength > StringAWhereSearch.Replace(StringBToSearch, string.Empty).Length;
            }
            return false;

        }

        public static bool IsStringContainsAnyExclusion(string InputString, string[] exclusions)
        {
            if (InputString.Length > 0 && exclusions != null)
            {
                int exclusionsLength = exclusions.Length;
                for (int i = 0; i < exclusionsLength; i++)
                {
                    if (IsStringAContainsStringB(InputString, exclusions[i]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsStringAequalsStringB(string StringA, string StringB, bool ignoreCase = false)
        {
            if (ignoreCase)
            {
                return string.Compare(StringA, StringB, ignoreCase, CultureInfo.InvariantCulture) == 0;
            }
            else
            {
                return string.CompareOrdinal(StringA, StringB) == 0;
            }
        }

        public static string AddStringBToAIfValid(string StringA, string StringB)
        {
            //добавление имени автора в начало имени папки
            if (StringA.StartsWith("[AI][", System.StringComparison.InvariantCulture) || (StringA.StartsWith("[", System.StringComparison.InvariantCulture) && !StringA.StartsWith("[AI]", System.StringComparison.InvariantCulture)) || ManageStrings.IsStringAContainsStringB(StringA, StringB))
            {
            }
            else if (StringB.Length > 0)
            {
                //проверка на любые невалидные для имени папки символы
                if (ManageFilesFolders.ContainsAnyInvalidCharacters(StringB))
                {
                }
                else
                {
                    StringA = "[" + StringB + "]" + StringA;
                }
            }

            return StringA;
        }

        public static bool IsStringAContainsAnyStringFromStringArray(string StringA, string[] StringArray, bool IgnoreCase = false)
        {
            foreach (var StringB in StringArray)
            {
                if (IgnoreCase)
                {
                    if (IsStringAContainsStringB(StringA, StringB) || IsStringAContainsStringB(StringA.ToUpperInvariant(), StringB.ToUpperInvariant()))
                    {
                        return true;
                    }
                }
                else
                {
                    if (IsStringAContainsStringB(StringA, StringB))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
