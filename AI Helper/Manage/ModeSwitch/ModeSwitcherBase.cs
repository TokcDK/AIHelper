using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIHelper.Manage.ModeSwitch
{
    abstract class ModeSwitcherBase
    {
        /// <summary>
        /// Text for switch confirmation dialog
        /// </summary>
        protected abstract string DialogText { get; }

        /// <summary>
        /// Method which will be executed
        /// </summary>
        protected abstract void Action();

        public async void Switch()
        {
            SharedData.GameData.MainForm.OnOffButtons(false);

            DialogResult result = MessageBox.Show(
                DialogText,
                T._("Confirmation"),
                MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                await Task.Run(() => Action()).ConfigureAwait(true);

                try
                {
                    SharedData.GameData.MainForm.MOCommonModeSwitchButton.Text = MOmode ? T._("MOToCommon") : T._("CommonToMO");
                }
                catch
                {
                }

                SharedData.GameData.MainForm.FoldersInit();
            }

            SharedData.GameData.MainForm.OnOffButtons();
        }

        protected virtual bool NeedSkip(string sourceFilePath, string sourceFolder)
        {
            return false;
        }

        protected virtual void PreParseFiles()
        {
        }

        protected bool MOmode { get => ManageSettings.IsMoMode(); set => Properties.Settings.Default.MOmode = value; }

        /// <summary>
        /// list of files with log paths
        /// </summary>
        protected List<string> longPaths;
        /// <summary>
        /// File paths list from vanilla Data folder with no mods
        /// </summary>
        protected string[] vanillaDataFilesList;
        /// <summary>
        /// List of empty folder paths in vanilla Data directory with no mods
        /// </summary>
        protected StringBuilder vanillaDataEmptyFoldersList;
        /// <summary>
        /// список guid zipmod-ов
        /// </summary>
        protected Dictionary<string, string> zipmodsGuidList;
        /// <summary>
        /// список выполненных операций с файлами.
        /// </summary>
        protected StringBuilder moToStandartConvertationOperationsList;
        /// <summary>
        /// True if any files was parsed
        /// </summary>
        protected bool ParsedAny;

        /// <summary>
        /// normal mode identifier switcher
        /// </summary>
        /// <param name="create">true=Create/false=Delete</param>
        protected void SwitchNormalModeIdentifier(bool create = true)
        {
            if (create)
            {
                if (!File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), "normal.mode")))
                {
                    File.WriteAllText(Path.Combine(ManageSettings.GetCurrentGameDataPath(), "normal.mode"), "The game is in normal mode");
                }
            }
            else
            {
                if (File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), "normal.mode")))
                {
                    File.Delete(Path.Combine(ManageSettings.GetCurrentGameDataPath(), "normal.mode"));
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <param name="parentDir">Parent directory</param>
        /// <returns></returns>
        protected bool ParseDirectories(string sourceFolder, string parentDir)
        {
            Parallel.ForEach(Directory.GetDirectories(sourceFolder), dir =>
            {
                if (dir.IsSymlink(ObjectType.Directory))
                {
                    ParseDirLink(dir, parentDir);
                }
                else
                {
                    ParseFiles(dir, parentDir);
                    ParseDirectories(sourceFolder, parentDir);
                }
            });

            return true;
        }

        protected void ParseDirLink(string dir, string parentDir)
        {
            if (dir.IsValidSymlink())
            {
                var symlinkTarget = Path.GetFullPath(dir.GetSymlinkTarget());

                var targetPath = dir.Replace(parentDir, ManageSettings.GetCurrentGameDataPath()); // we move to data
                symlinkTarget.CreateSymlink(targetPath, isRelative: true, objectType: ObjectType.Directory);
            }
        }

        protected bool ParseFiles(string dir, string parentDir)
        {
            var sourceFilePaths = Directory.GetFiles(dir, "*.*");
            if (sourceFilePaths.Length == 0)
            {
                return false;
            }

            PreParseFiles();

            var sourceFilePathsLength = sourceFilePaths.Length;
            for (int f = 0; f < sourceFilePathsLength; f++)
            {
                var sourceFilePath = sourceFilePaths[f];
                if (ManageStrings.CheckForLongPath(ref sourceFilePath))
                {
                    longPaths.Add(sourceFilePath.Remove(0, 4)); // add to lng paths list but with removed long path prefix
                }

                if (NeedSkip(sourceFilePath, parentDir))
                {
                    continue;
                }

                ParseFile(sourceFilePath, parentDir);
            }

            return true;
        }

        protected void ParseFile(string sourceFilePath, string sourceFolder)
        {
            var dataFilePath = sourceFilePath.Replace(sourceFolder, ManageSettings.GetCurrentGameDataPath());
            if (ManageStrings.CheckForLongPath(ref dataFilePath))
            {
                longPaths.Add(dataFilePath.Remove(0, 4));
            }

            if (File.Exists(dataFilePath))
            {
                var vanillaFileBackupTargetPath = dataFilePath.Replace(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetCurrentGameMOmodeDataFilesBakDirPath());

                if (!File.Exists(vanillaFileBackupTargetPath) && vanillaDataFilesList.Contains(dataFilePath))
                {
                    var bakfolder = Path.GetDirectoryName(vanillaFileBackupTargetPath);
                    try
                    {
                        Directory.CreateDirectory(bakfolder);

                        dataFilePath.MoveTo(vanillaFileBackupTargetPath);//перенос файла из Data в Bak, если там не было

                        ManageModOrganizer.SaveGuidIfZipMod(sourceFilePath, zipmodsGuidList);

                        sourceFilePath.MoveTo(dataFilePath);
                        moToStandartConvertationOperationsList.AppendLine(sourceFilePath + "|MovedTo|" + dataFilePath);//запись об операции будет пропущена, если будет какая-то ошибка
                    }
                    catch (Exception ex)
                    {
                        // when file is not exist in Data, but file in Bak is exist and file in sourceFolder also exists => return file from Bak to Data
                        if (!File.Exists(dataFilePath) && File.Exists(vanillaFileBackupTargetPath) && File.Exists(sourceFilePath))
                        {
                            File.Move(vanillaFileBackupTargetPath, dataFilePath);
                        }

                        ManageLogs.Log("Error occured while to common mode switch:" + Environment.NewLine + ex + "\r\npath=" + bakfolder + "\r\nData path=" + dataFilePath + "\r\nSource dir path=" + sourceFilePath);
                    }
                }
            }
            else
            {
                var destFolder = Path.GetDirectoryName(dataFilePath);
                try
                {
                    Directory.CreateDirectory(destFolder);

                    ManageModOrganizer.SaveGuidIfZipMod(sourceFilePath, zipmodsGuidList);

                    sourceFilePath.MoveTo(dataFilePath);//перенос файла из папки мода в Data
                    moToStandartConvertationOperationsList.AppendLine(sourceFilePath + "|MovedTo|" + dataFilePath);//запись об операции будет пропущена, если будет какая-то ошибка
                }
                catch (Exception ex)
                {
                    ManageLogs.Log("Error occured while to common mode switch:" + Environment.NewLine + ex + "\r\npath=" + destFolder + "\r\nData path=" + dataFilePath + "\r\nSource dir path=" + sourceFilePath);
                }
            }
        }

        /// <summary>
        /// replace variable to path in string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        protected string ReplaceVarsToPaths(string str)
        {
            return str
            .Replace(ManageSettings.VarCurrentGameDataPath(), ManageSettings.GetCurrentGameDataPath())
            .Replace(ManageSettings.VarCurrentGameModsPath(), ManageSettings.GetCurrentGameModsDirPath())
            .Replace(ManageSettings.VarCurrentGameMoOverwritePath(), ManageSettings.GetCurrentGameMoOverwritePath());
        }

        /// <summary>
        /// replace path to variable in string array
        /// </summary>
        /// <param name="sarr"></param>
        protected void ReplacePathsToVars(ref string[] sarr)
        {
            for (int i = 0; i < sarr.Length; i++)
            {
                sarr[i] = sarr[i]
                .Replace(ManageSettings.GetCurrentGameDataPath(), ManageSettings.VarCurrentGameDataPath())
                .Replace(ManageSettings.GetCurrentGameModsDirPath(), ManageSettings.VarCurrentGameModsPath())
                .Replace(ManageSettings.GetCurrentGameMoOverwritePath(), ManageSettings.VarCurrentGameMoOverwritePath());
            }
        }

        /// <summary>
        /// replace variable to path in string array
        /// </summary>
        /// <param name="sarr"></param>
        protected void ReplaceVarsToPaths(ref string[] sarr)
        {
            for (int i = 0; i < sarr.Length; i++)
            {
                sarr[i] = sarr[i]
                .Replace(ManageSettings.VarCurrentGameDataPath(), ManageSettings.GetCurrentGameDataPath())
                .Replace(ManageSettings.VarCurrentGameModsPath(), ManageSettings.GetCurrentGameModsDirPath())
                .Replace(ManageSettings.VarCurrentGameMoOverwritePath(), ManageSettings.GetCurrentGameMoOverwritePath());
            }
        }

        /// <summary>
        /// replace path to variable in string builder
        /// </summary>
        /// <param name="sb"></param>
        protected void ReplacePathsToVars(ref StringBuilder sb)
        {
            sb = sb
                .Replace(ManageSettings.GetCurrentGameDataPath(), ManageSettings.VarCurrentGameDataPath())
                .Replace(ManageSettings.GetCurrentGameModsDirPath(), ManageSettings.VarCurrentGameModsPath())
                .Replace(ManageSettings.GetCurrentGameMoOverwritePath(), ManageSettings.VarCurrentGameMoOverwritePath());
        }

        ///// <summary>
        ///// replace variable to path in string builder
        ///// </summary>
        ///// <param name="sb"></param>
        //static void ReplaceVarsToPaths(ref StringBuilder sb)
        //{
        //    sb = sb
        //        .Replace(ManageSettings.VarCurrentGameDataPath(), ManageSettings.GetCurrentGameDataPath())
        //        .Replace(ManageSettings.VarCurrentGameModsPath(), ManageSettings.GetCurrentGameModsDirPath())
        //        .Replace(ManageSettings.VarCurrentGameMoOverwritePath(), ManageSettings.GetCurrentGameMoOverwritePath());
        //}

        //static bool OfferToSkipTheFileConfirmed(string file)
        //{
        //    DialogResult result = MessageBox.Show(
        //        T._("Path to file is too long!") + Environment.NewLine
        //        + "(" + file + ")" + Environment.NewLine
        //        + T._("Long Path can cause mode switch error and mode will not be switched.") + Environment.NewLine
        //        + T._("Skip it?") + Environment.NewLine
        //        , T._("Too long file path"), MessageBoxButtons.YesNo);

        //    return result == DialogResult.Yes;
        //}

        protected void MoveVanillaFilesBackToData()
        {
            var mOmodeDataFilesBakDirPath = ManageSettings.GetCurrentGameMOmodeDataFilesBakDirPath();
            if (Directory.Exists(mOmodeDataFilesBakDirPath))
            {
                var filesInMOmodeDataFilesBak = Directory.GetFiles(mOmodeDataFilesBakDirPath, "*.*", SearchOption.AllDirectories);
                int filesInMOmodeDataFilesBakLength = filesInMOmodeDataFilesBak.Length;
                for (int f = 0; f < filesInMOmodeDataFilesBakLength; f++)
                {
                    if (string.IsNullOrWhiteSpace(filesInMOmodeDataFilesBak[f]))
                    {
                        continue;
                    }

                    var destFileInDataFolderPath = filesInMOmodeDataFilesBak[f].Replace(mOmodeDataFilesBakDirPath, ManageSettings.GetCurrentGameDataPath());
                    if (!File.Exists(destFileInDataFolderPath))
                    {
                        var destFileInDataFolderPathFolder = Path.GetDirectoryName(destFileInDataFolderPath);
                        if (!Directory.Exists(destFileInDataFolderPathFolder))
                        {
                            Directory.CreateDirectory(destFileInDataFolderPathFolder);
                        }
                        filesInMOmodeDataFilesBak[f].MoveTo(destFileInDataFolderPath);
                    }
                }

                //удаление папки, где хранились резервные копии ванильных файлов
                ManageFilesFoldersExtensions.DeleteEmptySubfolders(mOmodeDataFilesBakDirPath);
            }
        }
    }
}
