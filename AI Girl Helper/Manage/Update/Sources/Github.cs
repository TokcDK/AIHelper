using AIHelper.Manage.Update.Targets;

namespace AIHelper.Manage.Update.Sources
{
    class Github : SBase
    {
        public Github(updateInfo info) : base(info)
        {
        }

        internal override string ID => "updgit";

        internal override string GetFile()
        {
            throw new System.NotImplementedException();
        }

        internal override string GetLastVersion()
        {
            throw new System.NotImplementedException();
        }
    }
}
