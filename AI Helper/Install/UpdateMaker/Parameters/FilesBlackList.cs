using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Install.UpdateMaker.Parameters
{
    internal class BlacklistDirs : IUpdateMakerIniParameter
    {
        public string Name => "BlacklistDirs";

        public int Priority => 1000;

        public string iniValue { get; set; }

        public string Parse(string filePath)
        {
            foreach(var mask in iniValue.Split(','))
            {
            }

            return filePath;
        }
    }
}
