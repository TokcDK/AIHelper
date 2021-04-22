using AIHelper.Utils;
using System;
using System.IO;
using System.Windows.Forms;

namespace AIHelper.Manage
{
    class ManageLogs
    {
        internal readonly static string LogFilePath = Path.Combine(Properties.Settings.Default.ApplicationStartupPath, Application.ProductName + ".log");

        /// <summary>
        /// will write message in log
        /// </summary>
        /// <param name="Message">message itself</param>
        /// <param name="LogLevel">default -1 = all, 0 = debug, 1 = info, 2 = error</param>
        internal static void Log(string Message, int LogLevel = -1)
        {
            try
            {
                if (LogLevel >= -1)//here must be log level check
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
