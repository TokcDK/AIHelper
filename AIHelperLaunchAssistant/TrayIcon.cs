using System;
using System.Runtime.InteropServices;

namespace AIHelperLaunchAssistant
{
    /// <summary>
    /// The icon the assistant keeps in the notification area while the target program runs, and the
    /// window that receives its messages.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Shell_NotifyIcon is used directly rather than System.Windows.Forms.NotifyIcon. The whole reason
    /// the assistant exists is that it stays resident for the length of a game session, and a UI
    /// framework loaded for one icon would cost memory the entire time.
    /// </para>
    /// <para>
    /// The window it owns is a plain hidden top-level window, not a message-only one: the shell
    /// announces a restart with a broadcast, and a message-only window is not sent broadcasts. It
    /// is created without the visible style and stays that way, so the icon in the notification
    /// area is the only thing the user ever sees of this tool while the target program runs.
    /// </para>
    /// </remarks>
    internal sealed class TrayIcon : IAssistantUi, IDisposable
    {
        /// <summary>Sent by the shell to the window when something happens to the icon.</summary>
        private const uint CallbackMessage = Win32.WmApp + 1;

        /// <summary>Posts to the window to put the icon in the notification area.</summary>
        private const uint ShowMessage = Win32.WmApp + 2;

        /// <summary>Posts to the window to end the message loop.</summary>
        private const uint StopMessage = Win32.WmApp + 3;

        private const uint MenuIdStartHostNow = 1;
        private const uint MenuIdExitWithoutRestart = 2;

        private const uint IconId = 1;

        /// <summary>szTip holds 128 characters including the terminator.</summary>
        private const int TipLength = 128;

        private const string WindowClassName = "AIHelperLaunchAssistantTrayWindow";

        private readonly object _sync = new object();
        private readonly string _hostDisplayName;
        private readonly ToolLog _log;
        private readonly Win32.WindowProcedure _windowProcedure;

        private string _tooltip;
        private UserRequest? _request;
        private IntPtr _window;
        private IntPtr _icon;
        private uint _taskbarCreatedMessage;
        private bool _iconIsOwned;
        private bool _iconAdded;
        private bool _classRegistered;
        private bool _disposed;

        /// <param name="hostDisplayName">Name of the application the assistant will start again.</param>
        /// <param name="log">Where to report an icon the shell refused.</param>
        internal TrayIcon(string hostDisplayName, ToolLog log)
        {
            _hostDisplayName = string.IsNullOrWhiteSpace(hostDisplayName) ? HostApplicationLocator.HostProcessName : hostDisplayName;
            _log = log;
            _tooltip = ApplicationInfo.Name;

            // Kept in a field: the operating system holds only a pointer to this delegate, and a
            // collected delegate would be called into freed memory.
            _windowProcedure = WindowProcedure;
        }

        /// <summary>
        /// Creates the window the icon belongs to. Nothing is shown yet - the icon appears when
        /// <see cref="TargetStarted"/> is called.
        /// </summary>
        /// <returns><c>false</c> when the window could not be created, in which case there is no message loop to run.</returns>
        internal bool TryShow()
        {
            if (_disposed || _window != IntPtr.Zero)
            {
                return _window != IntPtr.Zero;
            }

            IntPtr instance = Win32.GetModuleHandle(null);

            var windowClass = new Win32.WindowClassEx();
            windowClass.Size = (uint)Marshal.SizeOf(typeof(Win32.WindowClassEx));
            windowClass.WindowProcedure = _windowProcedure;
            windowClass.InstanceHandle = instance;
            windowClass.ClassName = WindowClassName;

            if (Win32.RegisterClassEx(ref windowClass) == 0)
            {
                int error = Marshal.GetLastWin32Error();

                // 1410 is ERROR_CLASS_ALREADY_EXISTS: another icon in this process registered it, and
                // its window is usable all the same.
                const int ClassAlreadyExists = 1410;
                if (error != ClassAlreadyExists)
                {
                    _log.Warn("The notification area window class could not be registered (error " + error + ").");
                    return false;
                }
            }
            else
            {
                _classRegistered = true;
            }

            _window = Win32.CreateWindowEx(
                0, WindowClassName, WindowClassName, 0, 0, 0, 0, 0,
                IntPtr.Zero, IntPtr.Zero, instance, IntPtr.Zero);

            if (_window == IntPtr.Zero)
            {
                _log.Warn("The notification area window could not be created (error " + Marshal.GetLastWin32Error() + ").");
                return false;
            }

            _taskbarCreatedMessage = Win32.RegisterWindowMessage("TaskbarCreated");
            LoadIcon();

            return true;
        }

        public void TargetStarted(string targetDisplayName)
        {
            lock (_sync)
            {
                _tooltip = targetDisplayName + " is running - " + _hostDisplayName + " will start when it ends";
            }

            // Posted rather than called: this runs on the thread that waits for the target, and the
            // window belongs to the thread running the message loop.
            IntPtr window = _window;
            if (window != IntPtr.Zero)
            {
                Win32.PostMessage(window, ShowMessage, IntPtr.Zero, IntPtr.Zero);
            }
        }

        public UserRequest? TakeRequest()
        {
            lock (_sync)
            {
                UserRequest? request = _request;
                _request = null;
                return request;
            }
        }

        /// <summary>Runs the message loop until <see cref="Stop"/> is called.</summary>
        internal void RunMessageLoop()
        {
            if (_window == IntPtr.Zero)
            {
                return;
            }

            Win32.Message message;
            while (Win32.GetMessage(out message, IntPtr.Zero, 0, 0) > 0)
            {
                Win32.TranslateMessage(ref message);
                Win32.DispatchMessage(ref message);
            }
        }

        /// <summary>Ends the message loop. Safe to call from any thread.</summary>
        internal void Stop()
        {
            IntPtr window = _window;
            if (window != IntPtr.Zero)
            {
                Win32.PostMessage(window, StopMessage, IntPtr.Zero, IntPtr.Zero);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            RemoveIcon();

            if (_window != IntPtr.Zero)
            {
                Win32.DestroyWindow(_window);
                _window = IntPtr.Zero;
            }

            if (_icon != IntPtr.Zero)
            {
                // The icon taken from the executable is ours to release; the system's fallback icon
                // belongs to the system and must be left alone.
                if (_iconIsOwned)
                {
                    Win32.DestroyIcon(_icon);
                }

                _icon = IntPtr.Zero;
            }

            if (_classRegistered)
            {
                Win32.UnregisterClass(WindowClassName, Win32.GetModuleHandle(null));
                _classRegistered = false;
            }
        }

        private IntPtr WindowProcedure(IntPtr window, uint message, IntPtr wParam, IntPtr lParam)
        {
            // Not a constant: the shell registers this name at run time, so it is compared first.
            if (_taskbarCreatedMessage != 0 && message == _taskbarCreatedMessage)
            {
                // The shell restarted and forgot every icon it was showing; the assistant is still
                // here and has to put its own back.
                if (_iconAdded)
                {
                    AddIcon(CurrentTooltip());
                }

                return IntPtr.Zero;
            }

            switch (message)
            {
                case CallbackMessage:
                    HandleTrayMessage(lParam);
                    return IntPtr.Zero;

                case ShowMessage:
                    AddIcon(CurrentTooltip());
                    EnsureHidden();
                    return IntPtr.Zero;

                case StopMessage:
                    Win32.PostQuitMessage(0);
                    return IntPtr.Zero;

                case Win32.WmClose:
                    Win32.DestroyWindow(window);
                    return IntPtr.Zero;

                case Win32.WmDestroy:
                    Win32.PostQuitMessage(0);
                    return IntPtr.Zero;

                default:
                    return Win32.DefWindowProc(window, message, wParam, lParam);
            }
        }

        private void HandleTrayMessage(IntPtr lParam)
        {
            // The low word of the parameter carries the mouse message the shell is reporting.
            uint mouseMessage = unchecked((uint)(lParam.ToInt64() & 0xFFFF));

            switch (mouseMessage)
            {
                case Win32.WmLeftButtonDoubleClick:
                    Request(UserRequest.StartHostNow);
                    break;

                case Win32.WmRightButtonUp:
                case Win32.WmContextMenu:
                    ShowContextMenu();
                    break;
            }
        }

        private void ShowContextMenu()
        {
            IntPtr menu = Win32.CreatePopupMenu();
            if (menu == IntPtr.Zero)
            {
                return;
            }

            try
            {
                // A header line, so the menu says what the icon is waiting for. It is not selectable.
                Win32.AppendMenu(menu, Win32.MenuFlagString | Win32.MenuFlagDisabled, UIntPtr.Zero, CurrentTooltip());
                Win32.AppendMenu(menu, Win32.MenuFlagSeparator, UIntPtr.Zero, null);
                Win32.AppendMenu(menu, Win32.MenuFlagString, new UIntPtr(MenuIdStartHostNow), "Start " + _hostDisplayName + " now");
                Win32.AppendMenu(menu, Win32.MenuFlagString, new UIntPtr(MenuIdExitWithoutRestart), "Exit without starting " + _hostDisplayName);

                Win32.Point position;
                if (!Win32.GetCursorPos(out position))
                {
                    return;
                }

                // A menu shown from a notification icon is dismissed by the shell unless the window
                // that owns it is in the foreground first; this is the documented workaround, and the
                // trailing null message is the other half of it.
                Win32.SetForegroundWindow(_window);

                int command = Win32.TrackPopupMenu(
                    menu,
                    Win32.TrackPopupRightButton | Win32.TrackPopupReturnCommand | Win32.TrackPopupNoNotify,
                    position.X, position.Y, 0, _window, IntPtr.Zero);

                Win32.PostMessage(_window, Win32.WmNull, IntPtr.Zero, IntPtr.Zero);

                if (command == (int)MenuIdStartHostNow)
                {
                    Request(UserRequest.StartHostNow);
                }
                else if (command == (int)MenuIdExitWithoutRestart)
                {
                    Request(UserRequest.ExitWithoutRestartingHost);
                }
            }
            finally
            {
                Win32.DestroyMenu(menu);
            }
        }

        private void Request(UserRequest request)
        {
            lock (_sync)
            {
                _request = request;
            }
        }

        /// <summary>
        /// Keeps the assistant's own window off screen. Runs at the moment the target has started,
        /// which is the moment the assistant is supposed to be out of the way.
        /// </summary>
        /// <remarks>
        /// The window is created without the visible style, so this normally changes nothing. It is
        /// done anyway because the alternative is a stray window on top of the game, and the one
        /// thing this tool must never do is get between the user and what it was asked to start.
        /// Nothing is done about a console: the tool is built as a window application, so it has
        /// none, and hiding the console of whatever started it would take the caller's window away.
        /// </remarks>
        private void EnsureHidden()
        {
            IntPtr window = _window;
            if (window != IntPtr.Zero)
            {
                Win32.ShowWindow(window, Win32.ShowHide);
            }
        }

        private string CurrentTooltip()
        {
            lock (_sync)
            {
                return _tooltip;
            }
        }

        private void AddIcon(string tooltip)
        {
            if (_window == IntPtr.Zero || _icon == IntPtr.Zero)
            {
                return;
            }

            var data = new Win32.NotifyIconData();
            data.Size = (uint)Marshal.SizeOf(typeof(Win32.NotifyIconData));
            data.Window = _window;
            data.Id = IconId;
            data.Flags = Win32.NotifyIconFlagMessage | Win32.NotifyIconFlagIcon | Win32.NotifyIconFlagTip;
            data.CallbackMessage = CallbackMessage;
            data.Icon = _icon;
            data.Tip = Truncate(tooltip, TipLength - 1);

            _iconAdded = Win32.ShellNotifyIcon(Win32.NotifyIconAdd, ref data);
            if (!_iconAdded)
            {
                _log.Warn("The notification area did not accept the icon.");
            }
        }

        private void RemoveIcon()
        {
            if (!_iconAdded || _window == IntPtr.Zero)
            {
                return;
            }

            var data = new Win32.NotifyIconData();
            data.Size = (uint)Marshal.SizeOf(typeof(Win32.NotifyIconData));
            data.Window = _window;
            data.Id = IconId;

            Win32.ShellNotifyIcon(Win32.NotifyIconDelete, ref data);
            _iconAdded = false;
        }

        /// <summary>
        /// Takes the tool's own icon out of its executable, or the system's fallback icon when it
        /// cannot be read.
        /// </summary>
        private void LoadIcon()
        {
            try
            {
                IntPtr large;
                IntPtr small;
                if (Win32.ExtractIconEx(ApplicationInfo.ExecutablePath, 0, out large, out small, 1) > 0)
                {
                    if (large != IntPtr.Zero)
                    {
                        Win32.DestroyIcon(large);
                    }

                    if (small != IntPtr.Zero)
                    {
                        _icon = small;
                        _iconIsOwned = true;
                        return;
                    }
                }
            }
            catch (Exception exception) when (exception is InvalidOperationException || exception is NotSupportedException || exception is System.ComponentModel.Win32Exception)
            {
                _log.Warn("The tool's own icon could not be read: " + exception.Message);
            }

            _icon = Win32.LoadIcon(IntPtr.Zero, new IntPtr(Win32.DefaultApplicationIcon));
            _iconIsOwned = false;
        }

        private static string Truncate(string text, int maximumLength)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return text.Length <= maximumLength ? text : text.Substring(0, maximumLength);
        }
    }
}
