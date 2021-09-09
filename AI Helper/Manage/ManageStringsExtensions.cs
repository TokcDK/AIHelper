using System;
using System.Globalization;
using System.IO;

namespace AIHelper.Manage
{
    internal static class ManageStringsExtensions
    {
        /// <summary>
        /// Check if char is latin char
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsNotNeedToBeHexed(this char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c.IsDigit() || c.IsHTMLSymbol();
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

        /// <summary>
        /// Convert hexed string to normal string
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        internal static string UnHex(string inputString)
        {
            return System.Net.WebUtility.HtmlDecode(inputString);
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
                    if (arrayWhereReplace[i] == replaceWhat)
                    {
                        arrayWhereReplace[i] = replaceWith;
                    }
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
            if (!targetFileInfo.Exists)
            {
                return targetFileInfo;
            }

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
    }
}
