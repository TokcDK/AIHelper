using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AIHelper.Manage.Functions.BindMOProfileToExe
{
    internal class BindMOProfileToExeForFlp : FunctionForFlpBase
    {
        public override string Symbol => T._("P"); //࿊

        public override string Description => T._("Bind the Game or Studio to run with specific Mod Organizer profile.");

        public override void OnClick(object o, EventArgs e)
        {
            BindExeToMOProfile();
        }

        public override Color? ForeColor => Color.Yellow;

        internal static void BindExeToMOProfile()
        {
            Form bindForm = new BindForm();
            bindForm.Show();
        }
    }
}
