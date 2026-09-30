using System.Collections.Generic;

namespace AIHelper.Manage.Update.Providers
{
    /// <summary>
    /// Supplies the folders that can be updated together with the record saying
    /// where the new version of each one comes from.
    /// <para>
    /// There is one implementation per place this application has ever kept that
    /// information in — the hardcoded Mod Organizer record, updateinfo.txt and
    /// meta.ini — so nothing downstream has to know how many places there are or
    /// in which order they are consulted.
    /// </para>
    /// </summary>
    internal interface IUpdateRecordProvider
    {
        /// <summary>
        /// Folders this provider knows about, in the order they should be offered
        /// to the update pass.
        /// </summary>
        IEnumerable<UpdatableFolder> GetUpdatableFolders();
    }
}
