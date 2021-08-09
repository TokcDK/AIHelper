namespace AIHelper.Install.Types.Files.Archive.Extractors
{
    class RarExtractor : ArchiveExtractorBase
    {
        public override string[] Masks => new[] { "*.rar" };
    }
}
