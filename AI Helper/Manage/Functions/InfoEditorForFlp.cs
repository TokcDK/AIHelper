﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage.Functions
{
    internal class InfoEditorForFlp : FunctionForFlpBase
    {
        public override string Symbol => T._("⚒");

        public override string Description => T._("Open mods update info editor");

        public override void OnClick(object o, EventArgs e)
        {
            ManageInfoEditor.OpenInfoEditor();
        }
    }
}
