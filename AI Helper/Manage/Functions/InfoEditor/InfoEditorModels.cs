using System.Collections.Generic;
using AIHelper.Data.Modlist;

namespace AIHelper.Manage.Functions.InfoEditor
{
    /// <summary>
    /// One mod shown in the mods update info editor: the mod itself together with the data
    /// every update source holds for it.
    /// </summary>
    /// <remarks>
    /// The editor does not know which hosts a mod is updatable from; it asks each source,
    /// and what comes back is what this type holds. A mod which no source has data of is
    /// not shown at all, which is what keeps a mod folder with no update coordinates out
    /// of the list.
    /// </remarks>
    internal sealed class UpdateInfoData
    {
        internal UpdateInfoData(ModData mod)
        {
            Mod = mod;
            Sources = new List<UpdateSourceData>();
        }

        public ModData Mod { get; }

        public string ModName => Mod.Name;

        /// <summary>
        /// Data of every source which holds data for this mod, in the order the sources are
        /// registered. The block shows one tab per entry.
        /// </summary>
        internal List<UpdateSourceData> Sources { get; }
    }
}
