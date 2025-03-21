﻿using NLog;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIHelper.Manage.ModeSwitch
{
    class GameBackuper
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();
        string bakDirPath;
        string selectedGamePath;
        internal void CreateDataModsBakOfCurrentGame()
        {
            var frmProgress = new Form
            {
                Size = new Size(400, 50),
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.FixedToolWindow
            };
            var pbProgress = new ProgressBar
            {
                Dock = DockStyle.Bottom,
                Style = ProgressBarStyle.Marquee
            };

            frmProgress.Controls.Add(pbProgress);
            frmProgress.Text = T._("Creating backup of Data and Mods for the game") + ":" + ManageSettings.CurrentGameDirName;

            frmProgress.Show();

            bakDirPath = Path.Combine(selectedGamePath = ManageSettings.CurrentGameDirPath, "Bak", ManageSettings.CurrentGameDirName + "_backup" + ManageSettings.DateTimeBasedSuffix);

            Directory.CreateDirectory(bakDirPath);

            Parallel.ForEach(new[]
            {
                ManageSettings.CurrentGameDataDirPath,
                ManageSettings.CurrentGameModsDirPath,
            }, dir =>
            {
                BackupCreator2.CopyDirectoryWithLinks(dir, Path.Combine(bakDirPath, Path.GetFileName(dir)));
                //ParseDirectories(dir);
            });

            pbProgress.Dispose();
            frmProgress.Dispose();
        }
        void ParseDirectories(string sourceFolder)
        {
            ParseFiles(sourceFolder); // parse files of this directory

            Parallel.ForEach(Directory.EnumerateDirectories(sourceFolder), dir =>
            {
                dir = dir.ToLongPathWhenNeed(isFile: false);

                if (dir.IsSymlink(ObjectType.Directory))
                {
                    ParseDirLink(dir);
                }
                else
                {
                    ParseDir(dir);
                }
            });
        }

        private void ParseDir(string dir)
        {
            if (dir.IsSymlink(ObjectType.Directory))
            {
                var symlinkTarget = Path.GetFullPath(dir.GetSymlinkTarget(ObjectType.Directory));

                var targetPath = dir.Replace(selectedGamePath, ManageSettings.CurrentGameDataDirPath);
                targetPath = targetPath.ToLongPathWhenNeed(isFile: false);

                symlinkTarget.CreateSymlink(targetPath, isRelative: false, objectType: ObjectType.Directory);

            }
            else if (dir.IsEmptyDir())
            {
                var targetDirPath = dir.Replace(selectedGamePath, bakDirPath);
                targetDirPath = targetDirPath.ToLongPathWhenNeed(isFile: false);

                Directory.CreateDirectory(targetDirPath);
            }
            else
            {
                ParseDirectories(dir);
            }
        }

        void ParseDirLink(string dir)
        {
            if (dir.IsValidSymlink(objectType: ObjectType.Directory))
            {
                var symlinkTarget = Path.GetFullPath(dir.GetSymlinkTarget(ObjectType.Directory));

                var targetPath = dir.Replace(selectedGamePath, bakDirPath);
                targetPath = targetPath.ToLongPathWhenNeed(isFile: false);

                symlinkTarget.CreateSymlink(targetPath, isRelative: false, objectType: ObjectType.Directory);
            }
            else
            {
                var invalidSymlinkDirMarkerPath = dir + "_InvalidSymLink";
                if (!Directory.Exists(invalidSymlinkDirMarkerPath))
                {
                    Directory.CreateDirectory(invalidSymlinkDirMarkerPath);
                }
            }
        }

        void ParseFiles(string dir)
        {
            //var sourceFilePathsLength = sourceFilePaths.Length;
            Parallel.ForEach(Directory.EnumerateFiles(dir, "*.*"), sourceFilePath =>
            {
                ParseFile(sourceFilePath);
            });
        }
        protected void ParseFile(string sourceFilePath)
        {
            sourceFilePath = sourceFilePath.ToLongPathWhenNeed();

            var targetFilePath = sourceFilePath.Replace(selectedGamePath, bakDirPath);

            if (File.Exists(targetFilePath))
            {
                return;
            }
            targetFilePath = targetFilePath.ToLongPathWhenNeed();

            var targetFileParentDirPath = Path.GetDirectoryName(targetFilePath);
            targetFileParentDirPath = targetFileParentDirPath.ToLongPathWhenNeed(isFile: false);

            Directory.CreateDirectory(targetFileParentDirPath);

            try
            {
                if (sourceFilePath.IsSymlink(ObjectType.File))
                {
                    ParseFileLink(sourceFilePath);
                }
                else
                {
                    if(Path.GetPathRoot(sourceFilePath) == Path.GetPathRoot(targetFilePath))
                    {
                        sourceFilePath.CreateHardlink(targetFilePath);
                    }
                    else
                    {
                        sourceFilePath.CreateSymlink(targetFilePath, isRelative: false, objectType: ObjectType.File);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("Error occured while game buckup file move:" + Environment.NewLine + ex + "\r\npath=" + targetFileParentDirPath + "\r\nData path=" + targetFilePath + "\r\nSource dir path=" + sourceFilePath);
            }
        }

        void ParseFileLink(string linkPath)
        {
            if (linkPath.IsValidSymlink(objectType: ObjectType.File))
            {
                var symlinkTarget = Path.GetFullPath(linkPath.GetSymlinkTarget(ObjectType.File));

                var targetPath = linkPath.Replace(selectedGamePath, bakDirPath);
                symlinkTarget.CreateSymlink(targetPath, isRelative: false, objectType: ObjectType.File);
            }
            else
            {
                var invalidSymlinkDirMarkerPath = linkPath + ".InvalidSymLink";
                if (!File.Exists(invalidSymlinkDirMarkerPath))
                {
                    File.WriteAllText(invalidSymlinkDirMarkerPath, "Symlink file '" + linkPath + "' is invalid!");
                }
            }
        }
    }
}
