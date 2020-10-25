using static AIHelper.Manage.ManageUpdates;

namespace AIHelper.Manage.Update
{
    internal abstract class UpdateBase
    {
        protected UpdateData updateData;
        protected UpdateBase(UpdateData updateData)
        {
            this.updateData = updateData;
        }

        internal abstract bool Check();

        internal abstract void Update();
    }
}
