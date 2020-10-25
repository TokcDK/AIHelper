using AIHelper.Manage.Update.Targets;

namespace AIHelper.Manage.Update.Sources
{
    abstract class SBase : UBase
    {
        updateInfo info;
        protected SBase(updateInfo info)
        {
            this.info = info;
        }

        internal abstract string ID { get; } //id for identidy updateinfo like 'updgit::bbepis,XUnity.AutoTranslator,XUnity.AutoTranslator-BepIn-5x-::'

        internal abstract string GetLastVersion(); //function to get version of selected target

        internal abstract string GetFile(); //function of download update of selected file
    }
}
