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
        /// <param name="versionNew"></param>
        /// <param name="versionOld"></param>
        /// <returns></returns>
        public static bool IsNewerOf(this string versionNew, string versionOld, bool needClean = true)
        {
            if (string.IsNullOrWhiteSpace(versionNew))
            {
                return false;
            }
            else if (string.IsNullOrWhiteSpace(versionOld))
            {
                return true;
            }

            ManageModOrganizer.ConvertMODateVersion(ref versionNew);
            ManageModOrganizer.ConvertMODateVersion(ref versionOld);

            if (needClean)
            {
                // clean version for more correct comprasion
                CleanVersion(ref versionNew);
                CleanVersion(ref versionOld);
            }

            var versionString1Parts = versionNew.Split('.', ',');
            var versionString2Parts = versionOld.Split('.', ',');
            var versionString1PartsCount = versionString1Parts.Length;
            var versionString2PartsLastIndex = versionString2Parts.Length - 1;
            for (int i = 0; i < versionString1PartsCount; i++)
            {
                if (i > versionString2PartsLastIndex) // all digits was equal but current have smaller digits count
                {
                    return true;
                }

                // first try compare as int because string.compare will place 10 lower of 2 because first is 1
                if (int.TryParse(versionString1Parts[i], out int ver1)
                    && int.TryParse(versionString2Parts[i], out int ver2))
                {
                    if (ver1 > ver2) return true;
                    if (ver1 < ver2) return false; // nums are not equal and second newer of 1st
                }

                // then compare as ab order
                var compareResult = string.Compare(versionString1Parts[i], versionString2Parts[i]);
                if (compareResult == 1) // alphanumeric order of string 1 is higher of string 2
                {
                    return true;
                }
                else if (compareResult < 0) // strings are not equal and second is after 1st
                {
                    return false;
                }
            }

            return false;
        }
    }
}
