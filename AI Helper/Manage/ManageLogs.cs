﻿using AIHelper.Utils;
using System;
using System.IO;

namespace AIHelper.Manage
{
    class ManageLogs
    {
        internal readonly static string LogFilePath = Path.Combine(ManageSettings.ApplicationStartupPath, ManageSettings.ApplicationProductName + ".log");

        //[Obsolete]
        ///// <summary>
        ///// will write message in log
        ///// </summary>
        ///// <param name="message">message itself</param>
        ///// <param name="logLevel">default -1 = all, 0 = debug, 1 = info, 2 = error</param>
        //internal static void Log(string message, int logLevel = -1)
        //{
        //    try
        //    {
        //        if (logLevel >= -1)//here must be log level check
        //        {
        //            FileWriter.WriteData(
        //            LogFilePath
        //            , DateTime.Now + " >>" +
        //            message
        //            + Environment.NewLine, true);
        //        }
        //    }
        //    catch
        //    {
        //    }
        //}

        //[Obsolete]
        //internal static void Error(string message)
        //{
        //    Log(message: "An error occured - " + message, logLevel: 2);
        //}
    }
}
