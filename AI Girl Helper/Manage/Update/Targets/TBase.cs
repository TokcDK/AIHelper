using System.Collections.Generic;

namespace AIHelper.Manage.Update.Targets
{
    abstract class TBase
    {
        updateInfo info;

        protected TBase(updateInfo info)
        {
            this.info = info;
        }

        internal abstract Dictionary<string, string> GetUpdateInfos();//list of ModName\UpdateInfo pairs
    }
}
