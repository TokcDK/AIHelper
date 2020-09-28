using System.Globalization;

namespace AIHelper.Manage
{
    internal static class ManageStringsExtensions
    {
        internal static string ToLongPath(this string Path, bool IsFile = true, bool AlreadyChecked = false)
        {
            if (AlreadyChecked || (((IsFile && Path.Length > 259) || (!IsFile && Path.Length > 247)) && Path.Substring(0, 4) != @"\\?\"))
            {
                return @"\\?\" + Path;
            }
            else
            {
                return Path;
            }
        }

        internal static double GetProductVersionToFloatNumber(string FileProductVersion)
        {
            var v = FileProductVersion.Split('.');
            var Dot = false;
            var doubleString = string.Empty;
            foreach (var d in v)
            {
                doubleString += d;
                if (!Dot)
                {
                    doubleString += '.';
                    Dot = true;
                }
            }
            doubleString = doubleString.TrimEnd('0');
            return double.Parse(doubleString, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// replace element of the array
        /// </summary>
        /// <param name="ArrayWhereReplace"></param>
        /// <param name="ReplaceWhat"></param>
        /// <param name="ReplaceWith"></param>
        /// <param name="IsEqual">true=full value , false=contains</param>
        /// <returns></returns>
        internal static string[] Replace(this string[] ArrayWhereReplace, string ReplaceWhat, string ReplaceWith, bool IsEqual = true)
        {
            for (int i = 0; i < ArrayWhereReplace.Length; i++)
            {
                if (IsEqual)
                {
                    if (ArrayWhereReplace[i] == ReplaceWhat)
                    {
                        ArrayWhereReplace[i] = ReplaceWith;
                    }
                }
                else
                {
                    if (ManageStrings.IsStringAContainsStringB(ArrayWhereReplace[i], ReplaceWhat))
                    {
                        ArrayWhereReplace[i] = ArrayWhereReplace[i].Replace(ReplaceWhat, ReplaceWith);
                    }
                }
            }

            return ArrayWhereReplace;
        }
    }
}
