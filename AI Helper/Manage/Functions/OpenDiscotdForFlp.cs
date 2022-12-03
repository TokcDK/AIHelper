using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetListOfSubClasses;
using System.Windows.Forms;
using AIHelper.Games;
using System.Reflection;
using System.Diagnostics;
using System.Drawing;

namespace AIHelper.Manage.Functions
{
    internal class OpenDiscotdForFlp : FunctionForFlpBase
    {
        public override string Symbol => T._("🎮");

        public override string Description => string.Format(T._("Open 18+ Discord server of {0}", Application.ProductName));

        public override void OnClick(object o, EventArgs e)
        {
            Process.Start(ManageSettings.DiscordGroupLink);
        }
        public override Color? ForeColor => Color.DarkBlue;
    }
}
