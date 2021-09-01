using AIHelper.SharedData;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AIHelper.Manage.Update.Targets.Mods
{
    /// <summary>
    /// Base for mods
    /// </summary>
    abstract class ModsBase : Base
    {
        protected ModsBase(UpdateInfo info) : base(info)
        {
        }

        /// <summary>
        /// Update mod's folder with new files
        /// </summary>
        /// <returns></returns>
        internal override bool UpdateFiles()
        {
            return PerformUpdate();
        }

        internal override string GetParentFolderPath()
        {
            return ManageSettings.GetCurrentGameModsDirPath();
        }

        internal override void SetCurrentVersion()
        {
            var metaPath = Path.Combine(Info.TargetFolderPath.FullName, "meta.ini");
            if (File.Exists(metaPath))
            {
                Info.TargetCurrentVersion = ManageIni.GetIniValueIfExist(metaPath, "version", "General");
            }
        }

        internal override string[] RestorePathsList()
        {
            return new[]
            {
                @".\meta.ini"
            };
        }

        internal override string[] RestorePathsListExtra()        
        {
            string[,] objectLinkPaths = GameData.CurrentGame.GetDirLinkPaths();

            int objectLinkPathsLength = objectLinkPaths.Length / 2;
            HashSet<string> links = new HashSet<string>(objectLinkPathsLength);

            for (int i = 0; i < objectLinkPathsLength; i++)
            {
                var linkPath = objectLinkPaths[i, 1];

                if (links.Contains(linkPath))
                {
                    links.Add(linkPath);
                }
            }

            return links.ToArray();
        }
    }
}
