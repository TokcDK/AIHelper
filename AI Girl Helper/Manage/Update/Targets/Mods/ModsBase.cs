using System.IO;

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
    }
}
