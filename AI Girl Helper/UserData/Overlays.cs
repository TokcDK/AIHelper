using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Girl_Helper.UserData
{
    public class Overlays : IUserDataFolders
    {
        public string Foldername()
        {
            return "o";
        }

        public string TargetFolderSuffix()
        {
            return " Overlays";
        }

        public string Extension()
        {
            return ".png";
        }

        public string TypeFolder()
        {
            return string.Empty;
        }

        public string TargetFolderName()
        {
            return "Overlays";
        }
    }
}
