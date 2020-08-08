using AIHelper.Utils;
using System;
using System.IO;
using System.Windows.Forms;

namespace AIHelper.Manage
{
    class ManageLogs
    {
        internal readonly static string LogFilePath = Path.Combine(Properties.Settings.Default.ApplicationStartupPath, Application.ProductName + ".log");

        internal static void Log(string Message, int LogLevel = 0)
        {
            try
            {
                if (LogLevel < 1)
                {
                    FileWriter.WriteData(
                    LogFilePath
                    , DateTime.Now + " >>" +
                    Message
                    + Environment.NewLine, true);
                }
            }
            catch
            {
            }
        }
    }
}
