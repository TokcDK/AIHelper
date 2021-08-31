using System.Collections.Generic;
using System.IO;

namespace AIHelper.Install.Types.Directories
{
    class GameUpdateInstaller : DirectoriesInstallerBase
    {
        public override int Order => base.Order / 5;

        DirectoryInfo _dir;
        GameUpdateInfo updateInfo;
        protected override void Get(DirectoryInfo dir)
        {
            _dir = dir;
            var updateInfoIniFilePath = Path.Combine(dir.FullName, "updateinfo.ini");
            if (!File.Exists(updateInfoIniFilePath))
            {
                // not update
                return;
            }

            updateInfo = new GameUpdateInfo(updateInfoIniFilePath);

            if (string.IsNullOrWhiteSpace(updateInfo.GameFolderName))
            {
                // empty game folder name
                return;
            }

            if (Manage.ManageSettings.GetCurrentGameFolderName() != updateInfo.GameFolderName)
            {
                // incorrect game
                return;
            }

            if (updateInfo.UpdateMods == "true")
            {
                ProceedUpdateMods();
            }
        }

        private void ProceedUpdateMods()
        {
            if (!Directory.Exists(Path.Combine(_dir.FullName, "Games", updateInfo.GameFolderName, "Mods")))
            {
                return;
            }
        }
    }

    class GameUpdateInfo
    {
        Dictionary<string, string> keys = new Dictionary<string, string>()
        {
            { nameof(GameFolderName), "" },
            { nameof(UpdateData), "false" },
            { nameof(UpdateMods), "false" },
        };

        public GameUpdateInfo(string updateInfoPath)
        {
            Get(updateInfoPath);
        }

        internal string GameFolderName { get => keys[nameof(GameFolderName)]; set => keys[nameof(GameFolderName)] = value; }
        internal string UpdateData { get => keys[nameof(UpdateData)]; set => keys[nameof(UpdateData)] = value; }
        internal string UpdateMods { get => keys[nameof(UpdateMods)]; set => keys[nameof(UpdateMods)] = value; }

        void Get(string updateInfoPath)
        {
            var updateInfoIni = Manage.ManageIni.GetINIFile(updateInfoPath);

            foreach (var key in keys)
            {
                if (updateInfoIni.KeyExists(key.Key))
                {
                    keys[key.Key] = updateInfoIni.GetKey("", key.Key);
                }
            }
        }
    }
}
