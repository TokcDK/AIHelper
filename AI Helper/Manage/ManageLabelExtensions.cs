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
            label.ForeColor = checkState ? Checked : UnChecked;
        }

        /// <summary>
        /// Check if label has checked colors
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public static bool IsChecked(this Label label)
        {
            return label.ForeColor == Checked;
        }


    }
}
