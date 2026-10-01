namespace AIHelper.Install.Types.Files.Archive.Extractors
{
    class SevenZipExtractor : ArchiveExtractorBase
    {
        public override string[] Masks => new[] { "*.7z" };

        public override string ContentDescription => T._("7z archives (*.7z): extracted to a subfolder of the install folder");
    }
}
