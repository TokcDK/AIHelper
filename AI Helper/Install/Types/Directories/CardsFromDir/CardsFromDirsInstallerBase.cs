using AIHelper.Manage;
using System.IO;

namespace AIHelper.Install.Types.Directories.CardsFromDir
{
    abstract class CardsFromDirsInstallerBase : DirectoriesInstallerBase
    {
        protected override bool Get(DirectoryInfo dir)
        {
            var ret = false;
            foreach (var mask in Masks)
            {
                if(MoveImagesToTargetContentFolder(dir.FullName, mask))
                {
                    ret = true;
                }
            }

            return ret;

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

                //string theDirName = dir.Name;
                //if (ManageStrings.IsStringAequalsStringB(theDirName, "f", true))
                //{
                //    //папка "f" с женскими карточками внутри
                //    MoveImagesToTargetContentFolder(dir.FullName, "f");
                //}
                //else if (ManageStrings.IsStringAequalsStringB(theDirName, "m", true))
                //{
                //    //папка "m" с мужскими карточками внутри
                //    MoveImagesToTargetContentFolder(dir.FullName, "m");
                //}
                //else if (ManageStrings.IsStringAequalsStringB(theDirName, "c", true))
                //{
                //    //папка "c" с координатами
                //    MoveImagesToTargetContentFolder(dir.FullName, "c");
                //}
                //else if (ManageStrings.IsStringAequalsStringB(theDirName, "h", true)
                //    || ManageStrings.IsStringAequalsStringB(theDirName, "h1", true)
                //    || ManageStrings.IsStringAequalsStringB(theDirName, "h2", true)
                //    || ManageStrings.IsStringAequalsStringB(theDirName, "h3", true)
                //    || ManageStrings.IsStringAequalsStringB(theDirName, "h4", true)
                //    )
                //{
                //    //папка "h" с проектами домов
                //    MoveImagesToTargetContentFolder(dir.FullName, theDirName);
                //}
                //else if (ManageStrings.IsStringAequalsStringB(theDirName, "cf", true))
                //{
                //    //папка "cf" с передними фреймами
                //    MoveImagesToTargetContentFolder(dir.FullName, "cf");
                //}
                //else if (ManageStrings.IsStringAequalsStringB(theDirName, "cb", true))
                //{
                //    //папка "cb" с задними фреймами
                //    MoveImagesToTargetContentFolder(dir.FullName, "cb");
                //}
                //else if (ManageStrings.IsStringAequalsStringB(theDirName, "o", true))
                //{
                //    //папка "o" с оверлеями
                //    MoveImagesToTargetContentFolder(dir.FullName, "o");
                //}
                //else if (ManageStrings.IsStringAequalsStringB(theDirName, "s", true))
                //{
                //    //папка "s" с сценами для стидии
                //    MoveImagesToTargetContentFolder(dir.FullName, "s");
                //}
                //else
                //{
                //    if (!ManageFilesFolders.IsAnyFileExistsInTheDir(dir.FullName, "*.png"))
                //    {
                //        return;
                //    }

                //    //Просто произвольная подпапка, которая будет перемещена как новый мод
                //    MoveImagesToTargetContentFolder(dir.FullName, "f", true);
                //    //bool IsCharaCard = false;
                //    //foreach (var img in images)
                //    //{
                //    //    //var imgdata = Image.FromFile(img);

                //    //    //if (imgdata.Width == 252 && imgdata.Height == 352)
                //    //    //{
                //    //    //    IsCharaCard = true;
                //    //    //}
                //    //    string TargetPath = Path.Combine(GetCharactersFolder(), Path.GetFileName(img));
                //    //    File.Move(img, TargetPath);
                //    //}

                //    //string theDirName = Path.GetFileName(dir);
                //    var cardsModDir = Path.Combine(ManageSettings.GetCurrentGameModsPath(), theDirName);
                //    //var cardsModDir = GetResultTargetDirPathWithNameCheck(ManageSettings.GetCurrentGameModsPath(), Path.GetFileName(dir));

                //    //Перемещение файлов в ту же папку, если она существует, вместо создания новой
                //    if (Directory.Exists(cardsModDir))
                //    {
                //        foreach (var file in dir.GetFiles("*.*", SearchOption.AllDirectories))
                //        {
                //            string fileTarget = file.FullName.Replace(Properties.Settings.Default.Install2MODirPath, ManageSettings.GetCurrentGameModsPath());

                //            if (File.Exists(fileTarget))
                //            {
                //                fileTarget = ManageFilesFolders.GetResultTargetFilePathWithNameCheck(Path.GetDirectoryName(fileTarget), Path.GetFileNameWithoutExtension(fileTarget), Path.GetExtension(fileTarget));
                //            }
                //            file.MoveTo(fileTarget);
                //        }
                //        ManageFilesFolders.DeleteEmptySubfolders(dir.FullName);
                //    }
                //    else
                //    {
                //        dir.MoveTo(cardsModDir);
                //    }

                //    //запись meta.ini
                //    ManageModOrganizer.WriteMetaIni(
                //        cardsModDir
                //        ,
                //        ManageModOrganizer.GetCategoryIndexForTheName("Characters") + ","
                //        ,
                //        string.Empty
                //        ,
                //        string.Empty
                //        ,
                //        "<br>Author: " + string.Empty + "<br><br>" + Path.GetFileNameWithoutExtension(cardsModDir) + " character cards<br><br>"
                //        );

                //    ManageModOrganizer.ActivateDeactivateInsertMod(Path.GetFileName(cardsModDir), true, "UserCharacters_separator");
                //}
            }

        }

        /// <summary>
        /// suffix for target folder name
        /// </summary>
        protected abstract string TargetSuffix { get; }

        /// <summary>
        /// directory wich is placed in UserData
        /// </summary>
        protected virtual string typeFolder => string.Empty;

        /// <summary>
        /// Directory which is placed in typeFolder
        /// </summary>
        protected virtual string targetFolderName => string.Empty;

        /// <summary>
        /// extra actions
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="dir"></param>
        /// <param name="targetFolder"></param>
        /// <param name="moveInThisFolder"></param>
        protected virtual bool MoveByContentType(string contentType, string dir, string targetFolder, bool moveInThisFolder = false)
        {
            return false;
        }

        protected string extension = ".png";

        protected bool MoveImagesToTargetContentFolder(string dir, string contentType, bool moveInThisFolder = false)
        {
            var ret = false;
            
            string targetFolder = GetUserDataSubFolder(moveInThisFolder ? dir : " " + TargetSuffix, contentType);

            //Для всех, сброс png из корневой папки в целевую
            foreach (var target in Directory.GetFiles(dir, "*" + extension))
            {
                var cardframeTargetFolder = ManageFilesFolders.GetResultTargetFilePathWithNameCheck(targetFolder, Path.GetFileNameWithoutExtension(target), extension);

                ret = true;
                File.Move(target, cardframeTargetFolder);
            }

            MoveByContentType(contentType, dir, targetFolder, moveInThisFolder);

            ManageFilesFolders.DeleteEmptySubfolders(dir);

            return ret;
        }

        public static string GetUserDataSubFolder(string firstCandidateFolder, string type)
        {
            string[] targetFolders = new string[3]
            {
                firstCandidateFolder.Substring(0,1)== " " ? Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), "OrganizedModPack Downloaded"+firstCandidateFolder) : firstCandidateFolder,
                Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), "MyUserData"),
                ManageSettings.GetOverwriteFolder()
            };

            string typeFolder = string.Empty;
            string targetFolderName = string.Empty;
            switch (type)
            {
                case "f":
                    typeFolder = "chara";
                    targetFolderName = "female";
                    break;
                case "m":
                    typeFolder = "chara";
                    targetFolderName = "male";
                    break;
                case "c":
                    targetFolderName = "coordinate";
                    //TypeFolder = "";
                    break;
                case "h":
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                    typeFolder = "housing";
                    targetFolderName = type.Length == 2 ? "0" + type.Remove(0, 1) : string.Empty;
                    break;
                case "cf":
                    typeFolder = "cardframe";
                    targetFolderName = "Front";
                    break;
                case "cb":
                    typeFolder = "cardframe";
                    targetFolderName = "Back";
                    break;
                case "o":
                    //TypeFolder = "";
                    targetFolderName = "Overlays";
                    break;
                case "s":
                    typeFolder = "studio";
                    targetFolderName = "scene";
                    break;
            }

            int targetFoldersLength = targetFolders.Length;
            for (int i = 0; i < targetFoldersLength; i++)
            {
                string folder = targetFolders[i];
                if (Directory.Exists(folder))
                {
                    var targetResultDirPath = Path.Combine(folder, "UserData", typeFolder, targetFolderName);
                    if (!Directory.Exists(targetResultDirPath))
                    {
                        Directory.CreateDirectory(targetResultDirPath);
                    }
                    return targetResultDirPath;
                }
            }

            return Path.Combine(ManageSettings.GetOverwriteFolder(), "UserData", typeFolder, targetFolderName);
        }
    }
}
