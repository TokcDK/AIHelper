using System.Collections.Generic;

namespace AIHelper.Manage.Update.Targets.Mods
{
    /// <summary>
    /// Target for mods listed in the per-game updateinfo.txt file.
    /// </summary>
    class ModsList : ModsBase
    {
        public ModsList(UpdateInfo info) : base(info)
        {
        }

        /// <summary>
        /// Gets the enabled and existing mods from the update infos file.
        /// <para>
        /// NOTE: the "enabled mods only" flag is read from the static
        /// <c>ManageUpdateMods.UpdateOptions</c> field, unlike
        /// <see cref="ModsMeta"/> which reads
        /// <see cref="UpdateInfo.UpdateOptions"/>. The two targets can therefore
        /// disagree when the updater runs without an options dialog. Aligning
        /// them needs a signature change outside this folder.
        /// </para>
        /// </summary>
        /// <returns>The mods to check.</returns>
        internal override Dictionary<string, string> GetUpdateInfos()
        {
            return ManageUpdateMods.GetUpdateInfos(Info.SourceId);
        }
    }
}
