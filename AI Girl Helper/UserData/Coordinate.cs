using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Helper.UserData
{
    public class Coordinate : IUserDataFolders
    {
        public string Foldername()
        {
            return "m";
        }

        public string TargetFolderSuffix()
        {
            return " Coordinate";
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
            return "coordinate";
        }
    }
}
