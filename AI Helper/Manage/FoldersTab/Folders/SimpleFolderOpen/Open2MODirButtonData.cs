using AIHelper.Manage.ToolsTab.ButtonsData;

namespace AIHelper.Manage.FoldersTab.Folders
{
    internal class Open2MODirButtonData : SimpleFolderOpenBase
    {        
        public override string Text => "2MO";

        public override string Description => T._("Open folder, where from mod files can be installed fo selected game") +
                    T._("\n\nHere can be placed mod files which you want to install for selected game in approriate subfolders in mods" +
                    "\nand then can be installed all by one click on") + " " + new InstallModsButtonData().Text + " " + T._("button") +
                    "\n" + T._("which can be found in") + " " + ManageSettings.MainForm.ToolsTabPage.Text + " " + T._("tab page") +
                    "\n\n" + T._("Helper recognize") + ":"
                    + "\n " + T._(".dll files of BepinEx plugins")
                    + "\n " + T._("Sideloader mod archives")
                    + "\n " + T._("Female character cards")
                    + "\n " + T._("Female character cards in \"f\" subfolder")
                    + "\n " + T._("Male character cards in \"m\" subfolder")
                    + "\n " + T._("Coordinate clothes set cards in \"c\" subfolder")
                    + "\n " + T._("Studio scene cards in \"s\" subfolder")
                    + "\n " + T._("Cardframe Front cards in \"cf\" subfolder")
                    + "\n " + T._("Cardframe Back cards in \"cf\" subfolder")
                    + "\n " + T._("Script loader scripts")
                    + "\n " + T._("Housing plan cards in \"h\\01\", \"h\\02\", \"h\\03\" subfolders")
                    + "\n " + T._("Overlays cards in \"o\" subfolder")
                    + "\n " + T._("folders with overlays cards in \"o\" subfolder")
                    + "\n " + T._("Subfolders with modfiles")
                    + "\n " + T._("Zip archives with mod files")
                    + "\n\n" + T._("Any Rar and 7z archives will be extracted for install") +
                    T._("\nSome recognized mods can be updated instead of be installed as new mod") +
                    T._("\nMost of mods will be automatically activated except .cs scripts" +
                    "\nwhich always optional and often it is cheats or can slowdown/break game");

        protected override string DirPath => ManageSettings.Install2MoDirPath;
    }
}
