namespace AIHelper.Install.Types.Files.Archive.Extractors
{
    class RarExtractor : ArchiveExtractorBase
    {
        public override string[] Masks => new[] { "*.rar" };

        public override string ContentDescription => "Rar archives (*.rar): extracted to a subfolder of the install folder";
    }
}
