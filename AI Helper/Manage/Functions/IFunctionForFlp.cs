using System;
using System.Collections.Generic;
using System.Drawing;
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

    /// <summary>
    /// Extra parameter
    /// </summary>
    public interface IFunctionForFlpTextOptions
    {
        Color? ForeColor { get; } 
        Font Font { get; }
    }

    /// <summary>
    /// Optional part of a function description: whether the function can be used right now.
    /// </summary>
    /// <remarks>
    /// A function which cannot run is drawn in the colour the theme reserves for what is
    /// unavailable and does not react to a click, rather than failing when it is used. A
    /// description which does not implement this is always available, which keeps the check
    /// out of the functions which do not need it.
    /// </remarks>
    public interface IFunctionForFlpAvailability
    {
        bool IsAvailable { get; }
    }
}
