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
        /// <param name="latestVersion"></param>
        /// <param name="currentVersion"></param>
        /// <returns></returns>
        public static bool IsNewerOf(this string latestVersion, string currentVersion)
        {
            if (string.IsNullOrWhiteSpace(latestVersion))
            {
                return false;
            }
            else if (string.IsNullOrWhiteSpace(currentVersion))
            {
                return true;
            }

            var versionPartsOfLatest = latestVersion.TrimEnd('0', ',', '.').Split('.', ',');
            var versionPartsOfCurrent = currentVersion.TrimEnd('0', ',', '.').Split('.', ',');
            int dInd = 0;
            var curCount = versionPartsOfCurrent.Length;
            foreach (var digitL in versionPartsOfLatest)
            {
                if (curCount == dInd)//all digits was equal but current have smaller digits count
                {
                    return true;
                }
                var digitC = versionPartsOfCurrent[dInd];
                var latestParsed = int.TryParse(digitL, out int latest);
                var currentParsed = int.TryParse(digitC, out int current);
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
