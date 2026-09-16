using AIHelper.SharedData;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AIHelper.Manage.Update.Targets.Mods
{
    /// <summary>
    /// Base for mods. A mod is a folder below the current game's mods directory
    /// and its installed version is stored in meta.ini.
    /// </summary>
    abstract class ModsBase : UpdateTargetBase
    {
        protected ModsBase(UpdateInfo info) : base(info)
        {
        }

        /// <summary>
        /// Replaces the files of the current mod with the downloaded ones
        /// (see <see cref="UpdateTargetBase.PerformUpdate"/>).
        /// </summary>
        /// <returns>True when the mod was updated.</returns>
        internal override bool UpdateFiles()
        {
            return PerformUpdate();
        }

        internal override string GetParentFolderPath()
        {
            return ManageSettings.CurrentGameModsDirPath;
        }

        internal override void SetCurrentVersion()
        {
            var metaPath = Path.Combine(Info.TargetFolderPath.FullName, MetaIniFileName);

            if (!File.Exists(metaPath))
            {
                // without meta.ini there is no installed version to compare against
                Info.TargetCurrentVersion = string.Empty;
                return;
            }

            var version = ManageIni.GetIniValueIfExist(metaPath, MetaIniVersionKey, MetaIniVersionSection);
            ManageModOrganizer.ConvertMODateVersion(ref version);
            Info.TargetCurrentVersion = version;
        }

        internal override string[] RestorePathsList()
        {
            return new[]
            {
                "meta.ini"
            };
        }

        /// <summary>
        /// Link paths of the current game that may point into the mod being
        /// updated. The updater uses them to keep symlinked content in sync.
        /// </summary>
        /// <returns>The distinct link paths, or an empty array.</returns>
        internal override string[] RestorePathsListExtra()
        {
            var objectLinkPaths = ManageSettings.Games.Game.DirLinkPaths;

            var links = new HashSet<string>();

            if (objectLinkPaths == null) return new string[0];

            var rows = objectLinkPaths.GetLength(0);

            // the table is "mod side path" / "game side link path" pairs
            if (objectLinkPaths.GetLength(1) < 2) return new string[0];

            for (int i = 0; i < rows; i++)
            {
                var linkPath = objectLinkPaths[i, 1];

                if (string.IsNullOrWhiteSpace(linkPath)) continue;

                // HashSet de-duplicates the paths by itself
                links.Add(linkPath);
            }

            return links.ToArray();
        }
    }
}
