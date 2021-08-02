namespace AIHelper.Manage.Update.Sort
{
    abstract class SideloaderPackBase
    {
        internal abstract string DirName { get; }

        internal virtual string[] ParentDirPath { get => new[] { "mods" }; }

        internal virtual string[] FileNameFilter { get => new[] { "*.*" }; }
    }
}
