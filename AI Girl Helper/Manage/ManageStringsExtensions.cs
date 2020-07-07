using System.Globalization;

namespace AIHelper.Manage
{
    internal static class ManageStringsExtensions
    {
        internal static double GetProductVersionToFloatNumber(string FileProductVersion)
        {
            var v = FileProductVersion.Split('.');
            var Dot = false;
            var doubleString = string.Empty;
            foreach (var d in v)
            {
                doubleString += d;
                if (!Dot)
                {
                    doubleString += '.';
                    Dot = true;
                }
            }
            doubleString = doubleString.TrimEnd('0');
            return double.Parse(doubleString, CultureInfo.InvariantCulture);
        }
    }
}
