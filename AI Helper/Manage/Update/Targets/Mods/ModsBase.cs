﻿using AIHelper.SharedData;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AIHelper.Manage.Update.Targets.Mods
{
    /// <summary>
    /// Base for mods
    /// </summary>
    abstract class ModsBase : UpdateTargetBase
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
            return ManageSettings.CurrentGameModsDirPath;
        }

        internal override void SetCurrentVersion()
        {
            var metaPath = Path.Combine(Info.TargetFolderPath.FullName, "meta.ini");
            if (File.Exists(metaPath))
            {
                var version = ManageIni.GetIniValueIfExist(metaPath, "version", "General");
                ManageModOrganizer.ConvertMODateVersion(ref version);
                Info.TargetCurrentVersion = version;
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
            string[,] objectLinkPaths = ManageSettings.Games.Game.DirLinkPaths;

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
