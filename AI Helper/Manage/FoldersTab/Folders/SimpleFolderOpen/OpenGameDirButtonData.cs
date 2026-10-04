namespace AIHelper.Manage.FoldersTab.Folders
{
    internal class OpenGameDirButtonData : SimpleFolderOpenBase
    {
        public override string Text => T._("Game");

        public override string Description => T._("Open game dir");

        protected override string DirPath => ManageSettings.CurrentGameDataDirPath;
    }
}
