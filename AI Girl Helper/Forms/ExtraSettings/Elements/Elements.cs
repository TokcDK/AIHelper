using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIHelper.Forms.ExtraSettings.Elements
{
    internal abstract class Elements
    {
        internal Elements()
        {
            ElementInit();
        }

        internal virtual void ElementInit()
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
