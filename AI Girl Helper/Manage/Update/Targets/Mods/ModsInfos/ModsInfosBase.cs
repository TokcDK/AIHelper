namespace AIHelper.Manage.Update.Targets.Mods.ModsInfos
{
    abstract class ModsInfosBase
    {
        protected ModInfo modinfo;

        protected bool KK;
        protected bool HS;
        protected bool AI;
        protected bool HS2;

        protected ModsInfosBase(ModInfo modinfo)
        {
            this.modinfo = modinfo;

            //switch (ManageSettings.GetCurrentGameEXEName())
            //{
            //    case "Koikatu":
            //        KK = true;
            //        break;
            //    case "HoneySelect_64":
            //        HS = true;
            //        break;
            //    case "AI-Syoujyo":
            //        AI = true;
            //        break;
            //    case "HoneySelect2":
            //        HS2 = true;
            //        break;
            //}
        }

        /// <summary>
        /// check if valid mod
        /// </summary>
        internal abstract bool Check();

        /// <summary>
        /// owner name
        /// </summary>
        /// <returns></returns>
        internal abstract string Owner { get; }

        /// <summary>
        /// project name
        /// </summary>
        internal abstract string Project { get; }


        /// <summary>
        /// file name must start with this
        /// </summary>
        /// <returns></returns>
        internal abstract string StartsWith();

        /// <summary>
        /// file name can must end with this
        /// </summary>
        /// <returns></returns>
        internal virtual string EndsWith()
        {
            return "";
        }
    }
}
