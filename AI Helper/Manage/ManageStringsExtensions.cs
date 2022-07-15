using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AIHelper.Manage
{
    internal static class ManageStringsExtensions
    {
        /// <summary>
        /// Calculate count of char <paramref name="c"/> in input string <paramref name="s"/>
        /// </summary>
        /// <param name="s"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        internal static int CountOf(this string s, char c)
        {
            int count = -1;
            int index = -1;

            do
            {
                count++;
                index = s.IndexOf(c, index + 1);
            }
            while (index != -1);

            return count;
        }

        /// <summary>
        /// Check if char is latin char
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsNotNeedToBeHexed(this char c)
        {
            return IsLatinChar(c) || c.IsDigit() || c.IsHTMLSymbol();
        }

        /// <summary>
        /// <paramref name="inputChar"/> is symbol using as html tag mark
        /// </summary>
        /// <param name="inputChar"></param>
        /// <returns></returns>
        public static bool IsHTMLSymbol(this char inputChar)
        {
            string chars = "\\|!#$%&/=?»«\"'@£§€{}<>()-_:;,. ";
            foreach (var c in chars)
            {
                if (char.Equals(c, inputChar))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if char is latin char
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsLatinChar(this char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        /// <summary>
        /// Check if char is digit
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsDigit(this char c)
        {
            return !((c ^ '0') > 9);
        }

        //https://www.delftstack.com/howto/csharp/csharp-convert-string-to-hex/
        /// <summary>
        /// Convert string to hexed string
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        internal static string ToHex(this char c, bool xPrefix = true)
        {
            return (xPrefix ? @"\x" : "") + string.Format("{0:X2}", Convert.ToInt32(c));
        }

        //https://www.delftstack.com/howto/csharp/csharp-convert-string-to-hex/
        /// <summary>
        /// Convert string to hexed string
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        internal static string ToHex(this string inputString)
        {
            var s = new System.Text.StringBuilder();

            foreach (var c in inputString)
            {
                s.Append(c.ToHex());
            }

            return s.ToString();
        }

        /// <summary>
        /// Convert string to hexed string.
        /// Converts only required chars, not latin or symbols.
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        internal static string ToHexForMetaIni(this string inputString)
        {
            var s = new System.Text.StringBuilder();

            foreach (var c in inputString)
            {
                if (c.IsNotNeedToBeHexed())
                {
                    s.Append(c);
                }
                else
                {
                    s.Append(c.ToHex());
                }
            }

            return s.ToString();
        }

        ///// <summary>
        ///// Convert hexed string to normal string
        ///// </summary>
        ///// <param name="inputString"></param>
        ///// <returns></returns>
        //internal static string UnHex(this string inputString)
        //{
        //    return System.Net.WebUtility.HtmlDecode(inputString);
        //}

        /// <summary>
        /// Convert string like \x440 to char
        /// </summary>
        /// <param name="s"></param>
        /// <param name="xPrefix">true if string is starts with \x</param>
        /// <returns></returns>
        internal static char UnHex(this string s, bool xPrefix)
        {
            s = xPrefix ? s.Remove(0, 2) : s;

            var b = Convert.ToInt32(s, 16);
            var c = Convert.ToChar(b);

            return c;
        }
        internal static string UnHex(this string inputString)
        {
            var matches = Regex.Matches(inputString, @"\\x[0-9a-f]{3}");
            for (int i = matches.Count - 1; i >= 0; i--)
            {
                char unhexed = matches[i].Value.UnHex(true);
                inputString = inputString.Remove(matches[i].Index, matches[i].Length)
                    .Insert(matches[i].Index, unhexed.ToString());
            }

            return inputString;
        }

        /// <summary>
        /// Remove Mod Organizer html tags from notes string
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        internal static string StripMONotesHTML(this string inputString)
        {
            inputString = Regex.Replace(inputString, "<.*?>|&.*?;", "").Replace("\\n\\np, li { white-space: pre-wrap; }\\n\\n", "");

            if (inputString.StartsWith("\"")) inputString = inputString.Remove(0, 1);
            if(inputString.EndsWith("\"")) inputString = inputString.Remove(inputString.Length - 1);
            inputString = inputString.Replace(@"\x", "{xxx}");
            inputString = inputString.Replace("{xxx}", "\\x");

            inputString = inputString.Replace("<br/>", "\\n").Replace("<br>", "\\n").Replace("\n", "\\n").Replace("\r", "\\r");

            return inputString.UnHex();
        }

        /// <summary>
        /// check if path is long and can cause standart io operations errors.
        /// if path is long, will be added prefix "\\?\" for long paths.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isFile"></param>
        /// <param name="alreadyChecked"></param>
        /// <returns></returns>
        internal static string ToLongPathWhenNeed(this string path, bool isFile = true, bool alreadyChecked = false)
        {
            if (alreadyChecked || (((isFile && path.Length > 259) || (!isFile && path.Length > 247)) && path.Substring(0, 4) != @"\\?\"))
            {
                return @"\\?\" + path;
            }
            else
            {
                return path;
            }
        }

        internal static double GetProductVersionToFloatNumber(string fileProductVersion)
        {
            var v = fileProductVersion.Split('.');
            var dot = false;
            var doubleString = string.Empty;
            foreach (var d in v)
            {
                doubleString += d;
                if (!dot)
                {
                    doubleString += '.';
                    dot = true;
                }
            }
            doubleString = doubleString.TrimEnd('0');
            return double.Parse(doubleString, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// replace element of the array
        /// </summary>
        /// <param name="arrayWhereReplace"></param>
        /// <param name="replaceWhat"></param>
        /// <param name="replaceWith"></param>
        /// <param name="isEqual">true=full value , false=contains</param>
        /// <returns></returns>
        internal static string[] Replace(this string[] arrayWhereReplace, string replaceWhat, string replaceWith, bool isEqual = true)
        {
            for (int i = 0; i < arrayWhereReplace.Length; i++)
            {
                if (isEqual)
                {
                    if (arrayWhereReplace[i] == replaceWhat) arrayWhereReplace[i] = replaceWith;
                }
                else
                {
                    if (ManageStrings.IsStringAContainsStringB(arrayWhereReplace[i], replaceWhat))
                    {
                        arrayWhereReplace[i] = arrayWhereReplace[i].Replace(replaceWhat, replaceWith);
                    }
                }
            }

            return arrayWhereReplace;
        }

        /// <summary>
        /// Check if string A contains string B (if length of A > length of A with replaced B by "")<br></br><br></br>
        /// Speed check tests: https://cc.davelozinski.com/c-sharp/fastest-way-to-check-if-a-string-occurs-within-a-string
        /// </summary>
        /// <param name="stringAWhereSearch"></param>
        /// <param name="stringBToSearch"></param>
        /// <returns></returns>
        public static bool IsContains(this string stringAWhereSearch, string stringBToSearch)
        {
            int stringAInWhichSearchLength = stringAWhereSearch.Length;
            if (stringAInWhichSearchLength > 0 && stringBToSearch.Length > 0)//safe check for empty values
            {
                //if string A contains string B then string A with replaced stringB by empty will be
                return stringAInWhichSearchLength > stringAWhereSearch.Replace(stringBToSearch, string.Empty).Length;
            }
            return false;

        }

        /// <summary>
        /// Renames the file name so that the target path does not exist. 
        /// Name will be like "filename (#)"
        /// </summary>
        /// <param name="targetFileInfo">Input FileInfo of existing file</param>
        /// <returns></returns>
        public static FileInfo GetNewTargetName(this FileInfo targetFileInfo)
        {
            if (!targetFileInfo.Exists) return targetFileInfo;

            string name = Path.GetFileNameWithoutExtension(targetFileInfo.Name);
            int nameIndex = 1;
            string targetName = name;
            while (Directory.Exists(Path.Combine(targetFileInfo.DirectoryName, targetName, targetFileInfo.Extension)))
            {
                targetName = name + " (" + nameIndex + ")";
                nameIndex++;
            }

            return new FileInfo(Path.Combine(targetFileInfo.DirectoryName, targetName, targetFileInfo.Extension));
        }

        /// <summary>
        /// Check if <paramref name="inputString"/> contains any string from <paramref name="stringsCollection"/> elements
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="stringsCollection"></param>
        /// <returns></returns>
        public static bool ContainsAnyFrom(this string inputString, ICollection<string> stringsCollection)
        {
            foreach(var str in stringsCollection)
            {
                if (inputString.Contains(str)) return true;
            }

            return false;
        }
    }
}
