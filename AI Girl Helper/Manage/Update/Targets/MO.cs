using System.Collections.Generic;

namespace AIHelper.Manage.Update.Targets
{
    class MO : TBase
    {
        public MO(updateInfo info) : base(info)
        {
        }

        internal override Dictionary<string, string> GetUpdateInfos()
        {
            throw new System.NotImplementedException();
        }
    }
}
