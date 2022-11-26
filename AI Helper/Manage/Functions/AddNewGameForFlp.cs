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

namespace AIHelper.Manage.Functions
{
    internal class AddNewGameForFlp : IFunctionForFlp
    {
        public string Symbol => T._("🎮");

        public string Description => string.Format(T._("Add new game into {0} for management.", Application.ProductName));

        public void OnClick(object o, EventArgs e)
        {
            ManageOther.AddNewGame(ManageSettings.MainForm);
        }
    }
}
