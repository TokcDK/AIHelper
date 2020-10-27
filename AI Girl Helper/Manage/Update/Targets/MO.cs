using System.Collections.Generic;

namespace AIHelper.Manage.Update.Targets
{
    class MO : TBase
    {
        public MO(updateInfo info) : base(info)
        {
        }

        /// <summary>
        /// Get MO update info
        /// </summary>
        /// <returns></returns>
        internal override Dictionary<string, string> GetUpdateInfos()
        {
            return 
        }

        /// <summary>
        /// Update MO folder with new version
        /// </summary>
        /// <returns></returns>
        internal override bool UpdateFiles()
        {
            throw new System.NotImplementedException();
        }
    }
}
