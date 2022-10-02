using System;
using System.IO;
using static AIHelper.Manage.ManageModOrganizer;

namespace AIHelper.Manage.MOiniCustomExeFixers
{
    internal class ExplorerPPPathFixer : ICustomExePathFixerBase
    {
        public void TryFix(CustomExeFixData data)
        {
            if (data.Attribute == "binary")
            {
                if (File.Exists(data.Path)) return;

                foreach (var subPath in new[] { "/MO/explorer++/Explorer++.exe" })
                    if (TryFixSubPath(data, subPath, out var outPath))
                    {
                        if (File.Exists(outPath)) data.CustomExeData.Attribute[data.Attribute] = CustomExecutables.NormalizePath(outPath);
                    }
            }
            else if (data.Attribute == "workingDirectory")
            {
                if (Directory.Exists(data.Path)) return;

                foreach (var subPath in new[] { "/MO/explorer++" }) 
                    if (TryFixSubPath(data, subPath, out var outPath))
                    {
                        if (Directory.Exists(outPath)) data.CustomExeData.Attribute[data.Attribute] = CustomExecutables.NormalizePath(outPath);
                    }
            }
        }

        private bool TryFixSubPath(CustomExeFixData data, string subPath, out string outPath)
        {
            outPath = null;

            if (!data.Path.EndsWith(subPath, StringComparison.InvariantCultureIgnoreCase)) return false;

            outPath = ManageSettings.ApplicationStartupPath.TrimEnd('/').TrimEnd('\\').Replace("\\", "/") + subPath;

            return true;
        }
    }
}
