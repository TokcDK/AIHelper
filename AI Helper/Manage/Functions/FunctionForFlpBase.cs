using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage.Functions
{
    public abstract class FunctionForFlpBase : IFunctionForFlp
    {
        public abstract string Symbol { get; }

        public abstract string Description { get; }

        public abstract void OnClick(object o, EventArgs e);
    }
}
