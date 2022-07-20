using AIHelper.Manage;
using System.IO;
using System.Threading;

namespace AIHelper.Utils
{
    public static class FileWriter //example: https://stackoverflow.com/questions/47608949/c-sharp-multiple-threads-writing-to-the-same-file
    {
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        public static void WriteData(string filePath, string data, bool debugMode = false)
        {
            if (string.IsNullOrEmpty(filePath) || (!debugMode && filePath.Contains(ManageSettings.ApplicationProductName + ".log")))
            {
                return;
            }
            Locker.EnterWriteLock();
            try
            {
                File.AppendAllText(filePath, data);
            }
            finally
            {
                Locker.ExitWriteLock();
            }
        }
    }
}
