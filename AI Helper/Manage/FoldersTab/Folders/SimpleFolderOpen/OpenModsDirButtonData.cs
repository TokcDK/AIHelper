namespace AIHelper.Manage.FoldersTab.Folders
{
    internal class OpenModsDirButtonData : SimpleFolderOpenBase
    {
        public override string Text => T._("Mods");

        public override string Description => T._("Open Mods dir");

        protected override string DirPath => ManageSettings.CurrentGameModsDirPath;
    }
}
