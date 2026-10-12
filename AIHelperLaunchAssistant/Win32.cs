using System;
using System.Runtime.InteropServices;

namespace AIHelperLaunchAssistant
{
    /// <summary>
    /// The Win32 entry points the assistant uses, and nothing else.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Declared here instead of taken from System.Windows.Forms. The notification area is a handful of
    /// calls to Shell_NotifyIcon, and loading a whole UI framework to reach them would cost memory for
    /// as long as the assistant is resident - which is the length of a game session, and the reason the
    /// assistant exists at all.
    /// </para>
    /// <para>
    /// Every entry point that exists in a "W" and an "A" form is named in full. The runtime does not
    /// add the suffix on its own, and a name that does not exist fails at the first call rather than
    /// at the first build.
    /// </para>
    /// </remarks>
    internal static class Win32
    {
        internal const uint WmNull = 0x0000;
        internal const uint WmDestroy = 0x0002;
        internal const uint WmClose = 0x0010;
        internal const uint WmContextMenu = 0x007B;
        internal const uint WmLeftButtonDoubleClick = 0x0203;
        internal const uint WmRightButtonUp = 0x0205;
        internal const uint WmApp = 0x8000;

        internal const uint NotifyIconFlagMessage = 0x00000001;
        internal const uint NotifyIconFlagIcon = 0x00000002;
        internal const uint NotifyIconFlagTip = 0x00000004;
        internal const uint NotifyIconAdd = 0x00000000;
        internal const uint NotifyIconDelete = 0x00000002;

        internal const uint MenuFlagString = 0x00000000;
        internal const uint MenuFlagDisabled = 0x00000002;
        internal const uint MenuFlagSeparator = 0x00000800;

        internal const uint TrackPopupRightButton = 0x0002;
        internal const uint TrackPopupNoNotify = 0x0080;
        internal const uint TrackPopupReturnCommand = 0x0100;

        internal const uint MessageBoxOk = 0x00000000;
        internal const uint MessageBoxIconInformation = 0x00000040;

        internal const uint AttachParentProcess = 0xFFFFFFFF;
        internal const int DefaultApplicationIcon = 32512;
        internal const uint SnapshotProcesses = 0x00000002;

        /// <summary>Hides a window. Passed to <see cref="ShowWindow"/>.</summary>
        internal const int ShowHide = 0;

        /// <summary>Which handle <see cref="GetStdHandle"/> is asked for to find the output stream.</summary>
        internal const int StdOutputHandle = -11;

        /// <summary>Value CreateToolhelp32Snapshot returns when it failed.</summary>
        internal static readonly IntPtr InvalidHandleValue = new IntPtr(-1);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Point
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Message
        {
            public IntPtr Window;
            public uint Id;
            public IntPtr WParam;
            public IntPtr LParam;
            public uint Time;
            public Point Position;
        }

        /// <summary>
        /// The window procedure. The delegate has to stay referenced for as long as the window exists:
        /// the operating system holds only a raw pointer to it.
        /// </summary>
        internal delegate IntPtr WindowProcedure(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct WindowClassEx
        {
            public uint Size;
            public uint Style;
            public WindowProcedure WindowProcedure;
            public int ClassExtraBytes;
            public int WindowExtraBytes;
            public IntPtr InstanceHandle;
            public IntPtr IconHandle;
            public IntPtr CursorHandle;
            public IntPtr BackgroundBrush;
            public string MenuName;
            public string ClassName;
            public IntPtr SmallIconHandle;
        }

        /// <summary>
        /// NOTIFYICONDATAW as it is defined for Windows Vista and later. The size field has to be the
        /// size of this whole structure for the shell to read it.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct NotifyIconData
        {
            public uint Size;
            public IntPtr Window;
            public uint Id;
            public uint Flags;
            public uint CallbackMessage;
            public IntPtr Icon;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string Tip;

            public uint State;
            public uint StateMask;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string Info;

            public uint TimeoutOrVersion;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string InfoTitle;

            public uint InfoFlags;
            public Guid ItemGuid;
            public IntPtr BalloonIcon;
        }

        /// <summary>PROCESSENTRY32W, as filled in by Process32First and Process32Next.</summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct ProcessEntry32
        {
            public uint Size;
            public uint Usage;
            public uint ProcessId;
            public UIntPtr DefaultHeapId;
            public uint ModuleId;
            public uint ThreadCount;
            public uint ParentProcessId;
            public int PriorityClassBase;
            public uint Flags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string ExecutableFile;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegisterClassExW", SetLastError = true)]
        internal static extern ushort RegisterClassEx(ref WindowClassEx windowClass);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "UnregisterClassW", SetLastError = true)]
        internal static extern bool UnregisterClass(string className, IntPtr instanceHandle);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "CreateWindowExW", SetLastError = true)]
        internal static extern IntPtr CreateWindowEx(
            uint extendedStyle, string className, string windowName, uint style,
            int x, int y, int width, int height,
            IntPtr parent, IntPtr menu, IntPtr instanceHandle, IntPtr parameter);

        [DllImport("user32.dll", EntryPoint = "DestroyWindow", SetLastError = true)]
        internal static extern bool DestroyWindow(IntPtr window);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "DefWindowProcW")]
        internal static extern IntPtr DefWindowProc(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetMessageW")]
        internal static extern int GetMessage(out Message message, IntPtr window, uint firstMessage, uint lastMessage);

        [DllImport("user32.dll", EntryPoint = "TranslateMessage")]
        internal static extern bool TranslateMessage(ref Message message);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "DispatchMessageW")]
        internal static extern IntPtr DispatchMessage(ref Message message);

        [DllImport("user32.dll", EntryPoint = "PostQuitMessage")]
        internal static extern void PostQuitMessage(int exitCode);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "PostMessageW", SetLastError = true)]
        internal static extern bool PostMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegisterWindowMessageW")]
        internal static extern uint RegisterWindowMessage(string message);

        [DllImport("user32.dll", EntryPoint = "CreatePopupMenu", SetLastError = true)]
        internal static extern IntPtr CreatePopupMenu();

        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "AppendMenuW", SetLastError = true)]
        internal static extern bool AppendMenu(IntPtr menu, uint flags, UIntPtr itemId, string text);

        [DllImport("user32.dll", EntryPoint = "TrackPopupMenu", SetLastError = true)]
        internal static extern int TrackPopupMenu(IntPtr menu, uint flags, int x, int y, int reserved, IntPtr window, IntPtr rectangle);

        [DllImport("user32.dll", EntryPoint = "DestroyMenu", SetLastError = true)]
        internal static extern bool DestroyMenu(IntPtr menu);

        [DllImport("user32.dll", EntryPoint = "GetCursorPos", SetLastError = true)]
        internal static extern bool GetCursorPos(out Point point);

        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        internal static extern bool SetForegroundWindow(IntPtr window);

        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        internal static extern bool ShowWindow(IntPtr window, int command);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "LoadIconW")]
        internal static extern IntPtr LoadIcon(IntPtr instanceHandle, IntPtr iconName);

        [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
        internal static extern bool DestroyIcon(IntPtr icon);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "MessageBoxW")]
        internal static extern int MessageBox(IntPtr window, string text, string caption, uint type);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, EntryPoint = "Shell_NotifyIconW")]
        internal static extern bool ShellNotifyIcon(uint message, ref NotifyIconData data);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, EntryPoint = "ExtractIconExW")]
        internal static extern uint ExtractIconEx(string fileName, int iconIndex, out IntPtr largeIcon, out IntPtr smallIcon, uint iconCount);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetModuleHandleW")]
        internal static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32.dll", EntryPoint = "CreateToolhelp32Snapshot", SetLastError = true)]
        internal static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint processId);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "Process32FirstW", SetLastError = true)]
        internal static extern bool Process32First(IntPtr snapshot, ref ProcessEntry32 entry);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "Process32NextW", SetLastError = true)]
        internal static extern bool Process32Next(IntPtr snapshot, ref ProcessEntry32 entry);

        [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", EntryPoint = "AttachConsole", SetLastError = true)]
        internal static extern bool AttachConsole(uint processId);

        [DllImport("kernel32.dll", EntryPoint = "FreeConsole", SetLastError = true)]
        internal static extern bool FreeConsole();

        [DllImport("kernel32.dll", EntryPoint = "GetConsoleWindow")]
        internal static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true)]
        internal static extern IntPtr GetStdHandle(int standardHandle);

    }
}
