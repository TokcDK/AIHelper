using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage.Functions
{
    public abstract class FunctionForFlpBase : IFunctionForFlp, IFunctionForFlpTextOptions
    {
        public abstract string Symbol { get; }

        public abstract string Description { get; }

        public virtual Color? ForeColor => null;

        public virtual Font Font => null;

        public abstract void OnClick(object o, EventArgs e);
    }
}
