using AIHelper.SharedData;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AIHelper.Manage.Update.Targets.Mods
{
    /// <summary>
    /// Base for mods
    /// </summary>
    abstract class ModsBase : TBase
    {
        protected ModsBase(updateInfo info) : base(info)
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
            return ManageSettings.GetCurrentGameModsPath();
        }

        internal override void SetCurrentVersion()
        {
            var metaPath = Path.Combine(info.TargetFolderPath.FullName, "meta.ini");
            if (File.Exists(metaPath))
            {
                info.TargetCurrentVersion = ManageINI.GetINIValueIfExist(metaPath, "version", "General");
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
            string[,] ObjectLinkPaths = GameData.CurrentGame.GetDirLinkPaths();

            int ObjectLinkPathsLength = ObjectLinkPaths.Length / 2;
            HashSet<string> links = new HashSet<string>(ObjectLinkPathsLength);

            for (int i = 0; i < ObjectLinkPathsLength; i++)
            {
                var LinkPath = ObjectLinkPaths[i, 1];

                if (links.Contains(LinkPath))
                {
                    links.Add(LinkPath);
                }
            }

            return links.ToArray();
        }
    }
}
