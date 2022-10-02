using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AIHelper.Manage.ManageModOrganizer;

namespace AIHelper.Manage.MOiniCustomExeFixers
{
    public class RelativePathFixer : ICustomExePathFixerBase
    {
        public void TryFix(CustomExeFixData data)
        {
            if (!data.Path.StartsWith("..", StringComparison.InvariantCulture)) return;

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
