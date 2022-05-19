using System;
using System.Collections.Generic;
using System.IO;

namespace AIHelper.Manage.Update.Targets.Mods
{
    class ModsList : ModsBase
    {
        public ModsList(UpdateInfo info) : base(info)
        {
        }

        readonly string _updateInfosFile = ManageSettings.UpdateInfosFilePath;

        /// <summary>
        /// Get enabled and exists Mods list from updateinfos file
        /// </summary>
        /// <returns></returns>
        internal override Dictionary<string, string> GetUpdateInfos()
        {
            return ManageUpdateMods.GetUpdateInfos(Info.SourceId);
        }
    }
}
