using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Helper.UserData
{
    public class Housing : IUserDataFolders
    {
        public string Foldername()
        {
            return "h";
        }

        public string TargetFolderSuffix()
        {
            return " Housing";
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
            return "housing";
        }
    }
}
