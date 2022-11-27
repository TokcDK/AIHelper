
using System.Collections.Generic;
using AIHelper.Data.Modlist;

namespace AIHelper.Manage.Functions.ModlistFixes.Data
{
    internal class ModlistFixesData
    {
        public ModlistFixesData(ModData mod)
        {
            Mod = mod;
        }

        /// <summary>
        /// Currently parsing mod data
        /// </summary>
        internal ModData Mod { get; }


        internal List<string> Messages { get; } = new List<string>();
    }
}
