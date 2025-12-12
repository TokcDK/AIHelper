using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AIHelper.Manage.Update.Targets
{
    class Mo : UpdateTargetBase
    {
        public Mo(UpdateInfo info) : base(info)
        {
        }

        string _moDirPath;
        string _moExePath;
        private static readonly string[] _baseFilesToBackup = new[]
            {
                @".\ModOrganizer.ini",
                @".\categories.dat",
                @".\explorer++\Explorer++RU.dll",
                @".\explorer++\config.xml",
            };

        /// <summary>
        /// Get MO update info
        /// </summary>
        /// <returns></returns>
        internal override Dictionary<string, string> GetUpdateInfos()
        {
            if (Info.Source.Title.ToUpperInvariant().Contains("GITHUB"))
            {
                _moDirPath = ManageSettings.AppModOrganizerDirPath;
                return new Dictionary<string, string>()
                {
                    { _moDirPath, "ModOrganizer2,modorganizer,Mod.Organizer-"}
                };
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Update MO folder with new version
        /// </summary>
        /// <returns></returns>
        internal override bool UpdateFiles()
        {
            try
            {
                using (var installer = new Process())
                {
                    installer.StartInfo.FileName = Info.UpdateFilePath;
                    installer.StartInfo.Arguments = "/dir=\"" + _moDirPath + "\" /noicons /nocancel /norestart /silent";

                    installer.Start();
                    installer.WaitForExit();

                    RestoreSomeFiles(Info.BuckupDirPath, _moDirPath);
                    ManageModOrganizer.RestoreMissingModOrganizerBasicGamePluginFiles();

                    if (installer.ExitCode != 0)
                    {
                        File.Delete(Info.UpdateFilePath);//maybe corrupted
                    }

                    return installer.ExitCode == 0;
                }
            }
            catch
            {
                File.Delete(Info.UpdateFilePath);//maybe corrupted
                return false;
            }
        }

        internal override void SetCurrentVersion()
        {
            Info.UpdateFileEndsWith = ".exe";
            _moExePath = Path.Combine(_moDirPath, "ModOrganizer.exe");
            if (File.Exists(_moExePath))
            {
                Info.TargetCurrentVersion = FileVersionInfo.GetVersionInfo(_moExePath).ProductVersion;
            }
        }

        internal override string[] RestorePathsList()
        {
            return _baseFilesToBackup;
        }

        internal override bool MakeBuckup()
        {
            ManageModOrganizer.PreserveModOrganizerBasicGamePluginFiles();
            return base.MakeBuckup();
        }
    }
}
