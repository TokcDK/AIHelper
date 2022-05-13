using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AIHelper.Manage.Update.Targets
{
    class Mo : UpdateTargetBase
    {
        public Mo(UpdateInfo info) : base(info)
        {
        }

        string _moDirPath;
        string _moExePath;

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
                    //installer.StartInfo.WorkingDirectory = Path.GetDirectoryName(info.UpdateFilePath);

                    installer.Start();
                    installer.WaitForExit();

                    //UpdateBaseGamesPlugin();

                    RestoreSomeFiles(Info.BuckupDirPath, _moDirPath);

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

        //private void UpdateBaseGamesPlugin()
        //{
        //    var archive = info.source.DownloadFileFromTheLink(new Uri("https://github.com/ModOrganizer2/modorganizer-basic_games/archive/master.zip"));

        //    if (archive == null)
        //    {
        //        return;
        //    }

        //    try
        //    {
        //        var BaseGamesPluginPath = Path.Combine(ManageSettings.GetModsUpdateDirDownloadsPath(), "modorganizer-basic_games.zip");
        //        File.WriteAllBytes(BaseGamesPluginPath, archive);

        //        using (ZipArchive zip = ZipFile.OpenRead(info.UpdateFilePath))
        //        {
        //            zip.ExtractToDirectory(Path.Combine(ManageSettings.GetMOdirPath(), "plugins"));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ManageLogs.Log("error while modorganizer-basic_games extraction\r\n" + ex);
        //    }
        //}

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
            return new[]
            {
                @".\ModOrganizer.ini",
                @".\categories.dat",
                @".\explorer++\Explorer++RU.dll",
                @".\explorer++\config.xml",
            };
        }
    }
}
