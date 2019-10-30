using System.Diagnostics;
using System.Windows.Forms;

namespace AI_Girl_Helper
{
    public partial class LinksForm : Form
    {
        public LinksForm()
        {
            InitializeComponent();
            SetLocalization();
        }

        private void SetLocalization()
        {
            LinksCharactersGroupBox.Text = T._("Characters");
        }

        private void BooruLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://illusioncards.booru.org/index.php?page=post&s=list");
        }

        private void KenzatoLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://kenzato.uk/booru/category/AICARD");
        }

        private void IllusionDiscordLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://discord.gg/RfkZVS");
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://pastebin.com/QRRKtC45");
        }

        private void IllusionOficialUploader_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://mega.nz/#F!XOpkWahD!d0CpOSqiwww-M9QAVBjBSw");        
        }
    }
}
