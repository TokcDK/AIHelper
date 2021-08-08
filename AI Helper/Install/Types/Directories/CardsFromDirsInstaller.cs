using AIHelper.Manage;
using System.IO;

namespace AIHelper.Install.Types.Directories
{
    class CardsFromDirsInstaller : DirectoriesInstallerBase
    {
        protected override void Get(DirectoryInfo dir)
        {
            //string[] folderTypes = 
            //    {
            //    "f" //папка "f" с женскими карточками внутри
            //    ,
            //    "m" //папка "m" с мужскими карточками внутри
            //    ,
            //    "c" //папка "c" с координатами
            //    ,
            //    "h" //папка "h" с проектами домов
            //    ,
            //    "cf"//папка "cf" с передними фреймами
            //    ,
            //    "cb"//папка "cb" с задними фреймами
            //    ,
            //    "o" //папка "o" с оверлеями
            //    ,
            //    "s" //папка "s" с сценами для стидии
            //    };

            string theDirName = dir.Name;
            if (ManageStrings.IsStringAequalsStringB(theDirName, "f", true))
            {
                //папка "f" с женскими карточками внутри
                MoveImagesToTargetContentFolder(dir.FullName, "f");
            }
            else if (ManageStrings.IsStringAequalsStringB(theDirName, "m", true))
            {
                //папка "m" с мужскими карточками внутри
                MoveImagesToTargetContentFolder(dir.FullName, "m");
            }
            else if (ManageStrings.IsStringAequalsStringB(theDirName, "c", true))
            {
                //папка "c" с координатами
                MoveImagesToTargetContentFolder(dir.FullName, "c");
            }
            else if (ManageStrings.IsStringAequalsStringB(theDirName, "h", true)
                || ManageStrings.IsStringAequalsStringB(theDirName, "h1", true)
                || ManageStrings.IsStringAequalsStringB(theDirName, "h2", true)
                || ManageStrings.IsStringAequalsStringB(theDirName, "h3", true)
                || ManageStrings.IsStringAequalsStringB(theDirName, "h4", true)
                )
            {
                //папка "h" с проектами домов
                MoveImagesToTargetContentFolder(dir.FullName, theDirName);
            }
            else if (ManageStrings.IsStringAequalsStringB(theDirName, "cf", true))
            {
                //папка "cf" с передними фреймами
                MoveImagesToTargetContentFolder(dir.FullName, "cf");
            }
            else if (ManageStrings.IsStringAequalsStringB(theDirName, "cb", true))
            {
                //папка "cb" с задними фреймами
                MoveImagesToTargetContentFolder(dir.FullName, "cb");
            }
            else if (ManageStrings.IsStringAequalsStringB(theDirName, "o", true))
            {
                //папка "o" с оверлеями
                MoveImagesToTargetContentFolder(dir.FullName, "o");
            }
            else if (ManageStrings.IsStringAequalsStringB(theDirName, "s", true))
            {
                //папка "s" с сценами для стидии
                MoveImagesToTargetContentFolder(dir.FullName, "s");
            }
            else
            {
                if (!ManageFilesFolders.IsAnyFileExistsInTheDir(dir.FullName, "*.png"))
                {
                    return;
                }

                //Просто произвольная подпапка, которая будет перемещена как новый мод
                MoveImagesToTargetContentFolder(dir.FullName, "f", true);
                //bool IsCharaCard = false;
                //foreach (var img in images)
                //{
                //    //var imgdata = Image.FromFile(img);

                //    //if (imgdata.Width == 252 && imgdata.Height == 352)
                //    //{
                //    //    IsCharaCard = true;
                //    //}
                //    string TargetPath = Path.Combine(GetCharactersFolder(), Path.GetFileName(img));
                //    File.Move(img, TargetPath);
                //}

                //string theDirName = Path.GetFileName(dir);
                var cardsModDir = Path.Combine(Properties.Settings.Default.ModsPath, theDirName);
                //var cardsModDir = GetResultTargetDirPathWithNameCheck(Properties.Settings.Default.ModsPath, Path.GetFileName(dir));

                //Перемещение файлов в ту же папку, если она существует, вместо создания новой
                if (Directory.Exists(cardsModDir))
                {
                    foreach (var file in dir.GetFiles("*.*", SearchOption.AllDirectories))
                    {
                        string fileTarget = file.FullName.Replace(Properties.Settings.Default.Install2MODirPath, Properties.Settings.Default.ModsPath);

                        if (File.Exists(fileTarget))
                        {
                            fileTarget = ManageFilesFolders.GetResultTargetFilePathWithNameCheck(Path.GetDirectoryName(fileTarget), Path.GetFileNameWithoutExtension(fileTarget), Path.GetExtension(fileTarget));
                        }
                        file.MoveTo(fileTarget);
                    }
                    ManageFilesFolders.DeleteEmptySubfolders(dir.FullName);
                }
                else
                {
                    dir.MoveTo(cardsModDir);
                }

                //запись meta.ini
                ManageModOrganizer.WriteMetaIni(
                    cardsModDir
                    ,
                    ManageModOrganizer.GetCategoryIndexForTheName("Characters") + ","
                    ,
                    string.Empty
                    ,
                    string.Empty
                    ,
                    "<br>Author: " + string.Empty + "<br><br>" + Path.GetFileNameWithoutExtension(cardsModDir) + " character cards<br><br>"
                    );

                ManageModOrganizer.ActivateDeactivateInsertMod(Path.GetFileName(cardsModDir), true, "UserCharacters_separator");
            }
        }

        public static void MoveImagesToTargetContentFolder(string dir, string contentType, bool moveInThisFolder = false)
        {
            if (Directory.Exists(dir))
            {
                string targetFolder = string.Empty;
                string extension = ".png";
                if (contentType == "f")
                {
                    //TargetFolder = GetCharactersFolder();
                    targetFolder = ManageModOrganizerMods.GetUserDataSubFolder(moveInThisFolder ? dir : " Chars", contentType);
                }
                else if (contentType == "m")
                {
                    //TargetFolder = GetCharactersFolder(true);
                    targetFolder = ManageModOrganizerMods.GetUserDataSubFolder(moveInThisFolder ? dir : " Chars", contentType);
                }
                else if (contentType == "c")
                {
                    //TargetFolder = GetCoordinateFolder();
                    targetFolder = ManageModOrganizerMods.GetUserDataSubFolder(moveInThisFolder ? dir : " Coordinate", contentType);
                }
                else if (contentType == "h"
                    || contentType == "h1"
                    || contentType == "h2"
                    || contentType == "h3"
                    || contentType == "h4"
                    )
                {
                    //TargetFolder = GetCoordinateFolder();
                    targetFolder = ManageModOrganizerMods.GetUserDataSubFolder(moveInThisFolder ? dir : " Housing", contentType);
                }
                else if (contentType == "cf")
                {
                    //TargetFolder = GetCardFrameFolder();
                    targetFolder = ManageModOrganizerMods.GetUserDataSubFolder(moveInThisFolder ? dir : " Cardframes", contentType);
                }
                else if (contentType == "cb")
                {
                    //TargetFolder = GetCardFrameFolder(false);
                    targetFolder = ManageModOrganizerMods.GetUserDataSubFolder(moveInThisFolder ? dir : " Cardframes", contentType);
                }
                else if (contentType == "o")
                {
                    //TargetFolder = GetOverlaysFolder();
                    targetFolder = ManageModOrganizerMods.GetUserDataSubFolder(moveInThisFolder ? dir : " Overlays", contentType);
                }
                else if (contentType == "s")
                {
                    //TargetFolder = GetOverlaysFolder();
                    targetFolder = ManageModOrganizerMods.GetUserDataSubFolder(moveInThisFolder ? dir : " Scenes", contentType);
                }

                //Для всех, сброс png из корневой папки в целевую
                foreach (var target in Directory.GetFiles(dir, "*" + extension))
                {
                    var cardframeTargetFolder = ManageFilesFolders.GetResultTargetFilePathWithNameCheck(targetFolder, Path.GetFileNameWithoutExtension(target), extension);

                    File.Move(target, cardframeTargetFolder);
                }

                if (contentType == "o")
                {
                    ManageArchive.UnpackArchivesToSubfoldersWithSameName(dir, ".zip");
                    foreach (var oSubDir in Directory.GetDirectories(dir, "*"))
                    {
                        string newTarget = ManageFilesFolders.MoveFolderToOneLevelUpIfItAloneAndReturnMovedFolderPath(oSubDir);
                        string targetDirName = Path.GetFileName(newTarget);
                        var resultTargetPath = ManageFilesFolders.GetResultTargetDirPathWithNameCheck(targetFolder, targetDirName);

                        Directory.Move(newTarget, resultTargetPath);
                    }
                }
                else if (contentType == "f")
                {
                    string maleDir = Path.Combine(dir, "m");
                    if (Directory.Exists(maleDir))
                    {
                        foreach (var target in Directory.GetFiles(maleDir, "*.png"))
                        {
                            string name = Path.GetFileName(target);
                            File.Move(target, Path.Combine(dir, name));
                        }
                        Directory.Move(maleDir, maleDir + "_");
                        MoveImagesToTargetContentFolder(dir, "m", moveInThisFolder);
                    }
                }
                else if (contentType == "h"
                    || contentType == "h1"
                    || contentType == "h2"
                    || contentType == "h3"
                    || contentType == "h4"
                    )
                {
                    if (contentType.Length == 2)
                    {
                        foreach (var file in Directory.GetFiles(dir))
                        {
                            File.Move(file, ManageFilesFolders.GetResultTargetFilePathWithNameCheck(targetFolder, Path.GetFileNameWithoutExtension(file), ".png"));
                        }
                    }
                    else
                    {
                        foreach (var typeDir in Directory.GetDirectories(dir))
                        {
                            string hSubDirName = Path.GetFileName(typeDir);
                            foreach (var file in Directory.GetFiles(typeDir))
                            {
                                File.Move(file, ManageFilesFolders.GetResultTargetFilePathWithNameCheck(Path.Combine(targetFolder, hSubDirName), Path.GetFileNameWithoutExtension(file), ".png"));
                            }
                        }

                    }
                }

                ManageFilesFolders.DeleteEmptySubfolders(dir);
            }
        }
    }
}
