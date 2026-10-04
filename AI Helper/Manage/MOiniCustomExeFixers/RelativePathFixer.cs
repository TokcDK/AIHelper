using System;
using System.IO;
using static AIHelper.Manage.ManageModOrganizer;

namespace AIHelper.Manage.MOiniCustomExeFixers
{
    internal class RelativePathFixer : ICustomExePathFixerBase
    {
        public void TryFix(CustomExeFixData data)
        {
            if (string.IsNullOrWhiteSpace(data.Path)) return;

            // Only ".." as a whole segment is a relative path: either the value is nothing
            // but "..", or the two dots are followed by a separator. A name which merely
            // starts with two dots, such as "..config", is a normal name and must not be
            // rewritten. The comparison is Ordinal because a path is not culture.
            const string RelativePrefix = "..";

            if (!data.Path.StartsWith(RelativePrefix, StringComparison.Ordinal)) return;

            var afterPrefix = data.Path.Length > RelativePrefix.Length
                ? data.Path[RelativePrefix.Length].ToString()
                : string.Empty;

            if (afterPrefix.Length > 0 && afterPrefix != "\\" && afterPrefix != "/") return;

            //suppose relative path was from MO dir ..\%MODir%
            //replace .. to absolute path of current game directory
            var targetcorrectedrelative = data.Path
                    .Remove(0, 2).Insert(0, ManageSettings.CurrentGameDirPath);

            //replace other slashes
            var targetcorrectedabsolute = Path.GetFullPath(targetcorrectedrelative);

            data.CustomExeData.Attribute[data.Attribute] = CustomExecutables.NormalizePath(targetcorrectedabsolute);
        }
    }
}
