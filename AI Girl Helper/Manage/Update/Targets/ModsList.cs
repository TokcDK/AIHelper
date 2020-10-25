using System.Collections.Generic;

namespace AIHelper.Manage.Update.Targets
{
    class ModsList : TBase
    {
        public ModsList(updateInfo info) : base(info)
        {
        }

        internal override Dictionary<string, string> GetUpdateInfos()
        {
            throw new System.NotImplementedException();
        }
    }
}
