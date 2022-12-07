using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AIHelper.Manage.ui.themes
{
    internal class CherryBlackTheme : IUITheme
    {
        public string Name => "CherryBlack";

        readonly Color MainText = Color.White;
        readonly Color BGPanels = Color.FromArgb(255, 233, 76, 61);
        readonly Color BGOther = Color.FromArgb(255, 45, 41, 38);
        public List<ElementData> Elements => new List<ElementData>()
        {
            new ElementData()
            {
                Type = typeof(Panel),
                BackColor = BGPanels,
            },
            new ElementData()
            {
                Type = typeof(Button),
                BackColor = BGOther,
                ForeColor = MainText,
            },
            new ElementData()
            {
                Type = typeof(TabControl),
                BackColor = BGOther,
                ForeColor = MainText,
            },
            new ElementData()
            {
                Type = typeof(Label),
                ForeColor = MainText,
            },
            new ElementData()
            {
                Type = typeof(CheckBox),
                ForeColor = MainText,
            },
            new ElementData()
            {
                Type = typeof(TextBox),
                ForeColor = MainText,
                BackColor = BGOther,
            },
        };
    }
}
