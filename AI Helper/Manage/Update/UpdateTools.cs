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

            version = version.TrimFileVersion();

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
        /// <param name="LatestVersion"></param>
        /// <param name="CurrentVersion"></param>
        /// <returns></returns>
        public static bool IsNewerOf(this string LatestVersion, string CurrentVersion)
        {
            if (string.IsNullOrWhiteSpace(LatestVersion))
            {
                return false;
            }
            else if (string.IsNullOrWhiteSpace(CurrentVersion))
            {
                return true;
            }

            var VersionPartsOfLatest = LatestVersion.TrimEnd('0', ',', '.').Split('.', ',');
            var VersionPartsOfCurrent = CurrentVersion.TrimEnd('0', ',', '.').Split('.', ',');
            int dInd = 0;
            var curCount = VersionPartsOfCurrent.Length;
            foreach (var DigitL in VersionPartsOfLatest)
            {
                if (curCount == dInd)//all digits was equal but current have smaller digits count
                {
                    return true;
                }
                var DigitC = VersionPartsOfCurrent[dInd];
                var latestParsed = int.TryParse(DigitL, out int latest);
                var currentParsed = int.TryParse(DigitC, out int current);
                if (latestParsed && currentParsed)
                {
                    if (latest > current)
                    {
                        return true;
                    }
                    else if (latest < current)
                    {
                        return false;
                    }
                }
                dInd++;
            }
            return false;
        }
    }
}
