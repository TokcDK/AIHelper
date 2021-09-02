using System.IO;

namespace AIHelper.Manage.Update.Targets.Mods.ModsMetaUrl
{
    class Xua : ModsMetaUrlBase
    {
        public Xua(ModInfo modinfo) : base(modinfo)
        {
        }

        internal override string Owner => "bbepis";

        internal override string Project => "XUnity.AutoTranslator";

        internal override bool Check()
        {
            return Modinfo.Url.Contains("github.com/bbepis/XUnity.AutoTranslator");
        }

        internal override string StartsWith()
        {
            if (File.Exists(Path.Combine(Modinfo.Moddir.FullName, "BepInEx", "plugins", "XUnity.AutoTranslator", "XUnity.AutoTranslator.Plugin.Core.dll")))
            {
                return "XUnity.AutoTranslator-BepIn-5x-";
            }
            else if (File.Exists(Path.Combine(Modinfo.Moddir.FullName, "Plugins", "XUnity.AutoTranslator.Plugin.Core.dll")))
            {
                return "XUnity.AutoTranslator-IPA-";
            }

            return "";
        }
    }
}
