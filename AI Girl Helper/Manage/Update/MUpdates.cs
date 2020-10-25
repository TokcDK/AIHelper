using AIHelper.Manage.Update.Sources;
using AIHelper.Manage.Update.Targets;
using System.Collections.Generic;

namespace AIHelper.Manage.Update
{
    internal class updateInfo
    {
        internal string SourceID;
    }

    class MUpdates
    {
        internal void update()
        {
            updateInfo info = new updateInfo();
            var s = new List<SBase>//Sources of updates
            {
                new Github(info)
            };
            var t = new List<TBase>//
            {
                new MO(info),
                new ModsMeta(info),
                new ModsList(info)
            };

            foreach(var source in s)
            {
                info.SourceID = source.ID;
                foreach (var target in t)
                {
                    var Infos = target.GetUpdateInfos();
                }
            }

        }
    }
}
