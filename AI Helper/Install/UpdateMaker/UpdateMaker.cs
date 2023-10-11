using AIHelper.Manage;
using AIHelper.Manage.Update;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace AIHelper.Install.UpdateMaker
{
    partial class UpdateMaker
    {
        Dictionary<string, string> _gameupdatekeys = new Dictionary<string, string>();

        public bool MakeUpdate()
        {
            var updateMakeInfoFilePath = Path.Combine(ManageSettings.CurrentGameDirPath, "makeupdate.ini");
            if (!File.Exists(updateMakeInfoFilePath))
            {
                return false;
            }

            List<UpdateMakerBase> parameters = new List<UpdateMakerBase>()
            {
                new UpdateMakerMO(),
                new UpdateMakerMods(),
                new UpdateMakerData()
            };
            
            var infoIni = ManageIni.GetINIFile(updateMakeInfoFilePath);
            var updateDir = Path.Combine(ManageSettings.CurrentGameDirPath, "Updates", "Update");
            if (!InitUpdateDir(updateDir)) return false;

            var contentTypeParsers = new ContentTypeParserBase[]
            {
                new ContentTypeParserDirs(infoIni),
                new ContentTypeParserFiles(infoIni)
            };

            CopyAndAddUpdatePaths(parameters, contentTypeParsers, updateDir);

            WriteUpdateIni(infoIni, updateDir, contentTypeParsers);

            Process.Start(updateDir);

            return true;
        }

        private bool InitUpdateDir(string updateDir)
        {
            var dateTimeSuffix = ManageSettings.DateTimeBasedSuffix;
            if (Directory.Exists(updateDir))
            {
                try
                {
                    Directory.Move(updateDir, updateDir + dateTimeSuffix);
                }
                catch
                {
                    return false;
                }
            }
            Directory.CreateDirectory(updateDir);

            return true;
        }

        private void CopyAndAddUpdatePaths(List<UpdateMakerBase> updateMakers, ContentTypeParserBase[] contentTypeParsers, string updateDir)
        {
            var updateGameDir = Path.Combine(updateDir, "Games", ManageSettings.CurrentGameDirName);

            foreach (var updateMaker in updateMakers)
            {
                var parameterGameDir = Path.Combine(ManageSettings.CurrentGameDirPath, updateMaker.DirName);
                var parameterUpdateDir = Path.Combine(updateGameDir, updateMaker.DirName);

                foreach (var contentTypeParser in contentTypeParsers)
                {
                    contentTypeParser.UpdateMaker = updateMaker;

                    if (!contentTypeParser.IsNeedToCopy) continue;

                    contentTypeParser.Copy(parameterGameDir, parameterUpdateDir);

                    _gameupdatekeys.Add("Update" + updateMaker.DirName, updateMaker.IsAnyFileCopied.ToString().ToLowerInvariant());
                }
            }
        }

        private void WriteUpdateIni(INIFileMan.INIFile ini, string updateDir, ContentTypeParserBase[] contentTypeParsers)
        {
            var gameupdateini = ManageIni.GetINIFile(Path.Combine(updateDir, ManageSettings.GameUpdateInstallerIniFileName));
            gameupdateini.SetKey("", "GameFolderName", ManageSettings.CurrentGameDirName);
            gameupdateini.SetKey("", "IsRoot", "true");
            // add keys
            foreach (var parameter in _gameupdatekeys)
            {
                gameupdateini.SetKey("", parameter.Key, parameter.Value);
            }
            // add other missing keys and their default values
            foreach (var parameter in new Types.Directories.GameUpdateInfo().Keys)
            {
                if (!gameupdateini.KeyExists(parameter.Key, ""))
                {
                    gameupdateini.SetKey("", parameter.Key, parameter.Value);
                }
            }
            // set removelists
            foreach (var contentTypeParser in contentTypeParsers)
            {
                gameupdateini.SetKey("", contentTypeParser.RemoveKeyName, string.Join(",", contentTypeParser.RemoveList));
            }
            // set defaults from maker
            if (ini.SectionExistsAndNotEmpty("Default"))
            {
                foreach (var parameter in ini.GetSectionKeyValuePairs("Default"))
                {
                    gameupdateini.SetKey("", parameter.Key, parameter.Value);
                }
            }

            gameupdateini.WriteFile();
        }
    }
}
