using System.Text.RegularExpressions;

namespace AIHelper.Manage.Update
{
    public static class UpdateTools
    {
        /// <summary>
        /// Clean version from prefixes. change v13 to 13 and kind of
        /// </summary>
        /// <param name="version"></param>
        public static void CleanVersion(ref string version)
        {
            if (version.Length == 0)
                return;

            version = version.ToUpperInvariant()
                .TrimFileVersion()
                .Replace("ALPHA", "A")
                .Replace("BETA", "B")
                .Replace("RELEASE", "R")
                .Replace("PREVIEW", "P")
                .Replace("TEST", "")
                ;

            //foreach (var prefix in new[] { "VERSION", "VER", "V" })
            //{
            //    if (version.ToUpperInvariant().StartsWith(prefix, StringComparison.InvariantCulture))
            //    {
            //        version = version.Remove(0, prefix.Length);
            //        break;
            //    }
            //}
        }

        /// <summary>
        /// check if last version newer of current
        /// </summary>
        /// <param name="versionString1"></param>
        /// <param name="versionString2"></param>
        /// <returns></returns>
        public static bool IsNewerOf(this string versionString1, string versionString2, bool needClean = true)
        {
            if (string.IsNullOrWhiteSpace(versionString1))
            {
                return false;
            }
            else if (string.IsNullOrWhiteSpace(versionString2))
            {
                return true;
            }

            ManageModOrganizer.ConvertMODateVersion(ref versionString1);
            ManageModOrganizer.ConvertMODateVersion(ref versionString2);

            if (needClean)
            {
                // clean version for more correct comprasion
                CleanVersion(ref versionString1);
                CleanVersion(ref versionString2);
            }

            var versionString1Parts = versionString1.Split('.', ',');
            var versionString2Parts = versionString2.Split('.', ',');
            var versionString1PartsCount = versionString1Parts.Length;
            var versionString2PartsLastIndex = versionString2Parts.Length - 1;
            for (int i = 0; i < versionString1PartsCount; i++)
            {
                if (i > versionString2PartsLastIndex) // all digits was equal but current have smaller digits count
                {
                    return true;
                }

                if (string.Compare(versionString1Parts[i], versionString2Parts[i]) == 1) // alphanumeric order of string 1 is higher of string 2
                {
                    return true;
                }
            }

            return false;
        }
    }
}
