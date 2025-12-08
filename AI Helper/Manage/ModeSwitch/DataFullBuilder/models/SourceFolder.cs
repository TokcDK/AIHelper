namespace AIHelper.Manage.ModeSwitch.DataFullBuilder.Models
{
    public sealed class SourceFolder
    {
        public string Path { get; }
        public int Priority { get; }
        public string Name { get; }

        public SourceFolder(string path, int priority, string name)
        {
            Path = path;
            Priority = priority;
            Name = name;
        }
    }
}
