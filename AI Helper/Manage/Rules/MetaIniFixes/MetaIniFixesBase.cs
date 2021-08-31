using AIHelper.Manage.Rules.ModList;
using INIFileMan;

namespace AIHelper.Manage.Rules.MetaIniFixes
{
    abstract class ModlistDataMetaIniFixesBase
    {
        protected readonly ModListData modlistData;
        protected INIFile ini;
        protected string mod;

        public ModlistDataMetaIniFixesBase(ModListData modlistData, INIFile ini, string mod)
        {
            this.ini = ini;
            this.modlistData = modlistData;
            this.mod = mod;
        }

        internal abstract bool Apply();
    }
}
