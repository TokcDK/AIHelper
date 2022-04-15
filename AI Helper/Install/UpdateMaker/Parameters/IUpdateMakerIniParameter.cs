using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Install.UpdateMaker.Parameters
{
    public interface IUpdateMakerIniParameter
    {
        /// <summary>
        /// Parameter name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Priority in parse order, bigger priority earlier Parse will be executed
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// InitActions 
        /// </summary>
        string iniValue { get; set; }

        /// <summary>
        /// Parse file or dir path
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        string Parse(string filePath);
    }
}
