namespace AIHelper.Manage.ModeSwitch.DataFullBuilder.Models
{
    public sealed class FileEntry
    {
        public string RelativePath { get; }
        public string SourceAbsolutePath { get; }
        public int Priority { get; }

        public FileEntry(string relativePath, string sourceAbsolutePath, int priority)
        {
            RelativePath = relativePath;
            SourceAbsolutePath = sourceAbsolutePath;
            Priority = priority;
        }
    }
}
