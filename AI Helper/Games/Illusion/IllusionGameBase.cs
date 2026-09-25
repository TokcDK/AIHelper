using AIHelper.Manage;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace AIHelper.Games.Illusion
{
    public abstract class IllusionGameBase : GameBase
    {
        public override string RegistryPath => @"HKEY_CURRENT_USER\Software\illusion\" + GameExeName + @"\" + GameExeName;
        public override string RegistryInstallDirKey => "INSTALLDIR";

        public override string CharacterPresetsFolderSubPath => Path.Combine(UserDataDirName, "Chara");

        public override bool IsHaveSideloaderMods => true;
        public override UserControl GameSettingsControl => new IllusionGameSettingsUserControl();
        public override string GetGameConfigFilePath(string parentPath)
        {
            return Path.Combine(parentPath, UserDataDirName, "setup.xml");
        }

        /// <summary>
        /// Dir link pairs shared by the Illusion games that keep their user data
        /// in the Mod Organizer "MyUserData" folder: the character "cap" folder
        /// and the StudioNEO scene loader folder.
        /// </summary>
        /// <returns>A fresh list, so the paths follow the currently selected game.</returns>
        protected IReadOnlyList<PathPair> CreateMyUserDataDirLinks() => new[]
        {
            DirLink(
                Path.Combine(ManageSettings.CurrentGameModsDirPath, MyUserDataDirName, UserDataDirName, "cap"),
                Path.Combine(ManageSettings.CurrentGameDataDirPath, UserDataDirName, "cap")),
            DirLink(
                Path.Combine(ManageSettings.CurrentGameModsDirPath, MyUserDataDirName, UserDataDirName, "studioneo", "BetterSceneLoader"),
                Path.Combine(ManageSettings.CurrentGameDataDirPath, UserDataDirName, "studioneo", "BetterSceneLoader")),
        };
    }
}
