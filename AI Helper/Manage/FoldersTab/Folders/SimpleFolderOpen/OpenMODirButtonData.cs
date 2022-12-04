namespace AIHelper.Manage.FoldersTab.Folders
{
    internal class OpenMODirButtonData : SimpleFolderOpenBase
    {
        public override string Text => "MO";

        public override string Description => T._("Open Mod Organizer dir");

        protected override string DirPath => ManageSettings.AppModOrganizerDirPath;
    }
}
