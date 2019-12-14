using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.UserData
{
    public class CharaMale : IUserDataFolders
    {
        public string Foldername()
        {
            return "m";
        }

        public string TargetFolderSuffix()
        {
            return " Chars";
        }

        public string Extension()
        {
            return ".png";
        }

        public string TypeFolder()
        {
            return "Chara";
        }

        public string TargetFolderName()
        {
            return "male";
        }
    }
}
