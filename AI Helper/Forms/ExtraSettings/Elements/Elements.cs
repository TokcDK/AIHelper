using System.Windows.Forms;

namespace AIHelper.Forms.ExtraSettings.Elements
{
    internal abstract class Elements
    {
        internal Elements()
        {
        }

        internal virtual void Init()
        {
        }

        internal abstract string Title { get; }

        internal abstract bool Check();


        internal Form ElementToPlace;

        internal abstract Form ElementToShow
        {
            get;
            set;
        }

        internal abstract void Show(Form parentForm);
    }
}
