using System;
using System.IO;

namespace AIHelperLaunchAssistant
{
    /// <summary>
    /// Writes text where the user can see it, given that this is a window application with no console
    /// of its own.
    /// </summary>
    /// <remarks>
    /// Built as a window application on purpose, so that starting it never flashes a console window
    /// over the game. That also means there is no output stream to write to, so the console it was
    /// started from has to be attached first - the same thing Windows does for any GUI program run
    /// from a command prompt.
    /// </remarks>
    internal static class ConsoleBridge
    {
        /// <summary>
        /// Writes to the console the tool was started from, or shows a message box when there is none.
        /// For text the user asked for, such as the usage text.
        /// </summary>
        internal static void Show(string text)
        {
            if (WriteToConsole(text))
            {
                return;
            }

            Win32.MessageBox(IntPtr.Zero, text, ApplicationInfo.Name, Win32.MessageBoxOk | Win32.MessageBoxIconInformation);
        }

        /// <summary>
        /// Writes to the console when there is one, and says nothing when there is not. For text that
        /// the program which started the assistant should read, never the user.
        /// </summary>
        internal static bool TryWrite(string text)
        {
            return WriteToConsole(text);
        }

        private static bool WriteToConsole(string text)
        {
            // A redirected output stream is already bound to the right place, and a window
            // application started with one has no console window to attach to in the first place.
            if (HasOutputHandle() || Win32.GetConsoleWindow() != IntPtr.Zero)
            {
                Console.Out.Write(text);
                return true;
            }

            if (!Win32.AttachConsole(Win32.AttachParentProcess))
            {
                return false;
            }

            try
            {
                // The output stream was bound before the console existed, so it still points at
                // nothing; it has to be replaced before anything is written through it.
                var stdout = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
                Console.SetOut(stdout);
                Console.Out.Write(text);
                return true;
            }
            finally
            {
                Win32.FreeConsole();
            }
        }

        /// <summary>
        /// Whether there is an output stream to write to. A window application started without one
        /// gets a null handle rather than an error, and writing through it would silently go nowhere.
        /// </summary>
        private static bool HasOutputHandle()
        {
            IntPtr handle = Win32.GetStdHandle(Win32.StdOutputHandle);
            return handle != IntPtr.Zero && handle != Win32.InvalidHandleValue;
        }
    }
}
