namespace AIHelper.Manage.FoldersTab.Folders
{
    internal class OpenMOUserFilesDirButtonData : SimpleFolderOpenBase
    {
        public override string Text => "User files";

        public override string Description => T._("Open dir with user files");

        protected override string DirPath => ManageSettings.GetUserfilesDirectoryPath();
    }
}
