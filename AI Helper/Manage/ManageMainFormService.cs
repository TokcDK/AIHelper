using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIHelper.Manage
{
    internal class ManageMainFormService
    {
        internal static void CalcSizeDependOnDesktop(Form f)
        {
            var resolution = Screen.PrimaryScreen.Bounds;
            int w = (int)(resolution.Width / 3.3);
            f.Size = new Size(w, (int)(w * 0.6));
        }
    }
}
