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

                const string subPath = "/MO/explorer++/Explorer++.exe";

                if (TryFixSubPath(data, subPath, out var outPath)
                    && File.Exists(outPath))
                {
                    data.CustomExeData.Attribute[data.Attribute] = CustomExecutables.NormalizePath(outPath);
                }
            }
            else if (data.Attribute == "workingDirectory")
            {
                if (Directory.Exists(data.Path)) return;

                const string subPath = "/MO/explorer++";

                if (TryFixSubPath(data, subPath, out var outPath)
                    && Directory.Exists(outPath))
                {
                    data.CustomExeData.Attribute[data.Attribute] = CustomExecutables.NormalizePath(outPath);
                }
            }
        }

        private bool TryFixSubPath(CustomExeFixData data, string subPath, out string outPath)
        {
            outPath = null;

            // OrdinalIgnoreCase, not a culture comparison: a path is not text in a language.
            if (!data.Path.EndsWith(subPath, StringComparison.OrdinalIgnoreCase)) return false;

            outPath = ManageSettings.ApplicationStartupPath.TrimEnd('/').TrimEnd('\\').Replace("\\", "/") + subPath;

            return true;
        }
    }
}
