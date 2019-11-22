using AI_Girl_Helper.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Girl_Helper.Manage
{
    class MOModsManage
    {
        public static void CleanBepInExLinksFromData()
        {
            //удаление файлов BepinEx
            FileFolderOperations.DeleteIfSymlink(Path.Combine(Properties.Settings.Default.DataPath, "doorstop_config.ini"));
            FileFolderOperations.DeleteIfSymlink(Path.Combine(Properties.Settings.Default.DataPath, "winhttp.dll"));
            string BepInExDir = Path.Combine(Properties.Settings.Default.DataPath, "BepInEx");
            if (Directory.Exists(BepInExDir))
            {
                Directory.Delete(BepInExDir, true);
            }

            //удаление ссылок на папки плагинов BepinEx
            FileFolderOperations.DeleteIfSymlink(Path.Combine(Properties.Settings.Default.DataPath, "UserData", "MaterialEditor"), true);
            FileFolderOperations.DeleteIfSymlink(Path.Combine(Properties.Settings.Default.DataPath, "UserData", "Overlays"), true);
        }

        public static void BepinExLoadingFix()
        {
            if (Properties.Settings.Default.MOmode)
            {
                string[,] ObjectLinkPaths =
                {
                    {
                        Path.Combine(Properties.Settings.Default.ModsPath, "BepInEx5", "Bepinex", "core", "BepInEx.Preloader.dll")
                        ,
                        Path.Combine(Properties.Settings.Default.DataPath, "Bepinex", "core", "BepInEx.Preloader.dll")
                    }
                    ,
                    {
                        Path.Combine(Properties.Settings.Default.ModsPath, "BepInEx5", "doorstop_config.ini")
                        ,
                        Path.Combine(Properties.Settings.Default.DataPath, "doorstop_config.ini")
                    }
                    ,
                    {
                        Path.Combine(Properties.Settings.Default.ModsPath, "BepInEx5", "winhttp.dll")
                        ,
                        Path.Combine(Properties.Settings.Default.DataPath, "winhttp.dll")
                    }
                    ,
                    {
                        Path.Combine(Properties.Settings.Default.ModsPath, "MyUserData", "UserData", "MaterialEditor")
                        ,
                        Path.Combine(Properties.Settings.Default.DataPath, "UserData", "MaterialEditor")
                    }
                    ,
                    {
                        Path.Combine(Properties.Settings.Default.ModsPath, "MyUserData", "UserData", "Overlays")
                        ,
                        Path.Combine(Properties.Settings.Default.DataPath, "UserData", "Overlays")
                    }
                };

                int ObjectLinkPathsLength = ObjectLinkPaths.Length / 2;
                for (int i = 0; i < ObjectLinkPathsLength; i++)
                {
                    if (File.Exists(ObjectLinkPaths[i, 0]) && !File.Exists(ObjectLinkPaths[i, 1]))
                    {
                        FileFolderOperations.Symlink
                          (
                           ObjectLinkPaths[i, 0]
                           ,
                           ObjectLinkPaths[i, 1]
                           ,
                           true
                          );
                        FileFolderOperations.HideFileFolder(Path.Combine(Properties.Settings.Default.DataPath, "Bepinex"));
                    }
                }
            }
        }
    }
}
