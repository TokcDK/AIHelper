using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AI_Girl_Helper
{
    public partial class newform : Form
    {
        public newform()
        {
            InitializeComponent();
        }

        private void BooruLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://illusioncards.booru.org/index.php?page=post&s=list");
        }
    }
}
