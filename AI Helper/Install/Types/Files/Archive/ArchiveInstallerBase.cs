namespace AIHelper.Install.Types.Files.Archive
{
    abstract class ArchiveInstallerBase : FilesInstallerBase
    {
        // archives must be processed first because can be extracted to dirs
        public override int Order => base.Order / 10;
    }
}
