using System.IO;

namespace AIHelper.Manage.Update.Targets.Mods.ModsInfos
{
    class XUA : ModsInfosBase
    {
        public XUA(ModInfo modinfo) : base(modinfo)
        {
        }

        internal override string Owner => "bbepis";

        internal override string Project => "XUnity.AutoTranslator";

        internal override bool Check()
        {
            return modinfo.url.Contains("github.com/bbepis/XUnity.AutoTranslator");
        }

        internal override string StartsWith()
        {
            if(File.Exists(Path.Combine(modinfo.moddir.FullName, "BepInEx", "plugins", "XUnity.AutoTranslator", "XUnity.AutoTranslator.Plugin.Core.dll")))
            {
                return "XUnity.AutoTranslator-BepIn-5x-";
            }
            else if (File.Exists(Path.Combine(modinfo.moddir.FullName, "Plugins", "XUnity.AutoTranslator.Plugin.Core.dll")))
            {
                return "XUnity.AutoTranslator-IPA-";
            }

            return "";
        }
    }
}
