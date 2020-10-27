namespace AIHelper.Manage.Update.Targets.Mods
{
    /// <summary>
    /// Base for mods
    /// </summary>
    abstract class ModsBase : TBase
    {
        protected ModsBase(updateInfo info) : base(info)
        {
        }

        /// <summary>
        /// Update mod's folder with new files
        /// </summary>
        /// <returns></returns>
        internal override bool UpdateFiles() 
        {
            return PerformUpdate();
        }
    }
}
