using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage.Functions
{
    public interface IFunctionForFlp
    {
        string Symbol { get; }
        string Description { get; }

        void OnClick(object o, EventArgs e);
    }
}
