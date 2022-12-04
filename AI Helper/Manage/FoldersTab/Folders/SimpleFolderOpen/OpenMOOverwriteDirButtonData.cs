namespace AIHelper.Manage.FoldersTab.Folders
{
    internal class OpenMOOverwriteDirButtonData : SimpleFolderOpenBase
    {
        public override string Text => "Overwrite";

        public override string Description => T._("Open Overwrite dir");

        protected override string DirPath => ManageSettings.CurrentGameOverwriteFolderPath;
    }
}
