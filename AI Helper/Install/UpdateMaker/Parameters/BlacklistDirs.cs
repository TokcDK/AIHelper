namespace AIHelper.Install.UpdateMaker.Parameters
{
    /// <summary>
    /// Blacklist of the dirs which must be excluded from the update
    /// </summary>
    /// <remarks>
    /// Not used by <see cref="UpdateMaker"/> yet: blacklists are read by the content type parsers directly.
    /// </remarks>
    public class BlacklistDirs : IUpdateMakerIniParameter
    {
        public string Name => "BlacklistDirs";

        public int Priority => 1000;

        public string IniValue { get; set; }

        public string Parse(string filePath)
        {
            // TODO: parsing of the dirs blacklist is not implemented yet
            return filePath;
        }
    }
}
