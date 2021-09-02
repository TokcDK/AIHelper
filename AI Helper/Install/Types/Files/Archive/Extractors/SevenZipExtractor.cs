namespace AIHelper.Install.Types.Files.Archive.Extractors
{
    class SevenZipExtractor : ArchiveExtractorBase
    {
        public override string[] Masks => new[] { "*.7z" };
    }
}
