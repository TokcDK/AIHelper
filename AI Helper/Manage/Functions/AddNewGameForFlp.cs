﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetListOfSubClasses;
using System.Windows.Forms;
using AIHelper.Games;
using System.Reflection;
using System.Drawing;

namespace AIHelper.Manage.Functions
{
    internal class AddNewGameForFlp : FunctionForFlpBase
    {
        public override string Symbol => T._("G+"); //🎮

        public override string Description => string.Format(T._("Add new game into {0} for management.", Application.ProductName));

        public override void OnClick(object o, EventArgs e)
        {
            ManageOther.AddNewGame(ManageSettings.MainForm);
        }

        public override Color? ForeColor => Color.LightGreen;
    }
}
