using System.Drawing;
using System.Windows.Forms;

namespace AIHelper.Manage
{
    static class ManageLabelExtensions
    {
        public static readonly Color Checked = Color.FromArgb(192, 255, 192);
        public static readonly Color UnChecked = Color.LightGray;

        public static void SetCheck(this Label label, bool checkState)
        {
            if (checkState)
            {
                label.ForeColor = Checked;
            }
            else
            {
                label.ForeColor = UnChecked;
            }
        }

        /// <summary>
        /// Check if label has checked colors
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public static bool IsChecked(this Label label)
        {
            if (label.ForeColor == Checked)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


    }
}
