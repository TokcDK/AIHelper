﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Helper.UserData
{
    class CardframeBack : IUserDataFolders
    {
        public string Foldername()
        {
            return "cf";
        }

        public string TargetFolderSuffix()
        {
            return " Cardframes";
        }

        public string Extension()
        {
            return ".png";
        }

        public string TypeFolder()
        {
            return "cardframe";
        }

        public string TargetFolderName()
        {
            return "Back";
        }
    }
}