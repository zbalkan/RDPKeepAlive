using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace RDPKeepAlive
{
    internal static class KeepAlive
    {
        private const int ClassNameCapacity = 128;

        private const int WindowTitleCapacity = 128;

        private static readonly string[] _rdpClients = [
            "TscShellContainerClass", // MSTSC.EXE
            "WindowsForms10.Window.8.app.0.1d2098a_r8_ad1" // Sysinternals RDCMAN
            ];

        private static bool _clientIsNotTopmost;

        private static bool _found;

        private static IntPtr _originalForegroundWindow;

        private static string _rdpClientClassName = string.Empty;

        private static string _rdpClientWindowTitle = string.Empty;

        private static IntPtr _windowInFront;

        /// <summary>
        /// Finds the RDP client window by enumerating all top-level windows.
        /// </summary>
        /// <param name="client">The RDP client window.</param>
        /// <returns>True if an RDP client window is found; otherwise, false.</returns>
        internal static bool TryGetRDPClient(out Client client)
        {
            _found = false; // Reset the flag
            _ = NativeMethods.EnumWindows(EnumRDPWindowsProc, IntPtr.Zero);

            client = new Client
            {
                ClassName = _rdpClientClassName,
                WindowTitle = _rdpClientWindowTitle
            };

            return _found;
        }

        /// <summary>
        /// Executes the keep-alive process for the RDP client window.
        /// This method finds the specific RDP window, prepares mouse movement parameters,
        /// takes a snapshot of the current window state, processes the mouse movement to keep the RDP session active,
        /// and then restores the original window state.
        /// </summary>
        internal static void Execute()
        {
            // Find the specific RDP window
            var clientWindow = NativeMethods.FindWindowExW(IntPtr.Zero, IntPtr.Zero, _rdpClientClassName, _rdpClientWindowTitle);
            if (clientWindow == IntPtr.Zero)
            {
                return;
            }

            if (!TryGetMouseMovementParams(out var input))
            {
                Console.WriteLine("ERROR: TryGetMouseMovementParams returned false!");
                Console.WriteLine(GetErrorMessage());
                return;
            }

            TakeSnapshot(clientWindow);

            ProcessMouseMovement(clientWindow, input);

            RestoreSnapshot(clientWindow);
        }

        /// <summary>
        ///     Callback method invoked by EnumWindows for each top-level window. Identifies RDP
        ///     windows and simulates mouse movement to keep them active.
        /// </summary>
        /// <param name="hWnd">
        ///     Handle to a window.
        /// </param>
        /// <param name="lParam">
        ///     Application-defined value.
        /// </param>
        /// <returns>
        ///     True to continue enumeration; False to stop.
        /// </returns>
        private static bool EnumRDPWindowsProc(IntPtr hWnd, IntPtr lParam)
        {
            // Retrieve the class name of the window
            if (TryGetWindowClass(hWnd, out var className) && TryGetWindowTitle(hWnd, out var windowTitle) && _rdpClients.Contains(className))
            {
                _found = true;
                _rdpClientClassName = className;
                _rdpClientWindowTitle = windowTitle;
                return false; // Stop enumeration
            }
            return true;
        }

        private static string GetErrorMessage()
        {
            var win32Exception = new Win32Exception(Marshal.GetLastWin32Error());
            return win32Exception != null ? win32Exception.Message : "Unknown Error";
        }

        private static IntPtr GetWindowInFront(nint clientWindow, uint pidClient)
        {
            uint pidNext = 0;
            var next = clientWindow;
            while (pidNext == 0 || pidClient == pidNext)
            {
                next = NativeMethods.GetWindow(next, 3 /*GW_HWNDPREV*/);

                _ = NativeMethods.GetWindowThreadProcessId(next, out pidNext);
            }
            return next;
        }

        private static void ProcessMouseMovement(nint clientWindow, NativeMethods.INPUT input)
        {
            if (_clientIsNotTopmost)
            {
                // Bring the RDP window to the foreground
                NativeMethods.SetForegroundWindow(clientWindow);
            }

            // Send the mouse movement input
            if (NativeMethods.SendInput(1, ref input, Marshal.SizeOf(typeof(NativeMethods.INPUT))) == 0)
            {
                Console.WriteLine("ERROR: SendInput failed!");
                Console.WriteLine(GetErrorMessage());
            }
        }

        private static void RestoreSnapshot(nint clientWindow)
        {
            if (!_clientIsNotTopmost)
            {
                return;
            }

            // Restore the original foreground window
            NativeMethods.SetForegroundWindow(_originalForegroundWindow);

            // Put RDP client into previous place
            NativeMethods.SetWindowPos(
               clientWindow,
               _windowInFront,
               0, 0, 0, 0,
               NativeMethods.SetWindowPosFlags.NoMove |
               NativeMethods.SetWindowPosFlags.NoSize |
               NativeMethods.SetWindowPosFlags.NoActivate);
        }

        private static void TakeSnapshot(nint clientWindow)
        {
            // Store the original foreground window to restore later
            _originalForegroundWindow = NativeMethods.GetForegroundWindow();

            _clientIsNotTopmost = _originalForegroundWindow != clientWindow;

            if (!_clientIsNotTopmost)
            {
                return;
            }
            _ = NativeMethods.GetWindowThreadProcessId(clientWindow, out var pidClient);

            // Get the window in front of the RDP client
            _windowInFront = GetWindowInFront(clientWindow, pidClient);
        }

        private static bool TryGetMouseMovementParams(out NativeMethods.INPUT inputParams)
        {
            // Prepare INPUT structure for mouse movement
            inputParams = new NativeMethods.INPUT
            {
                type = NativeMethods.InputType.INPUT_MOUSE,
                U = new NativeMethods.InputUnion
                {
                    mi = new NativeMethods.MOUSEINPUT()
                }
            };

            // Get current cursor position
            if (!NativeMethods.GetCursorPos(out var currentPosition))
            {
                return false; // Continue enumeration despite the error
            }

            // Set mouse movement flags: Absolute positioning and movement
            inputParams.U.mi.dwFlags = NativeMethods.MouseEventFlags.MOVE | NativeMethods.MouseEventFlags.ABSOLUTE;

            // Calculate normalized absolute coordinates (0 to 65535)
            var screenWidth = NativeMethods.GetSystemMetrics(NativeMethods.SystemMetric.SM_CXSCREEN);
            var screenHeight = NativeMethods.GetSystemMetrics(NativeMethods.SystemMetric.SM_CYSCREEN);

            inputParams.U.mi.dx = (currentPosition.X * 65535) / screenWidth;
            inputParams.U.mi.dy = (currentPosition.Y * 65535) / screenHeight;
            return true;
        }

        private static bool TryGetWindowClass(IntPtr hWnd, out string className)
        {
            var name = new StringBuilder(ClassNameCapacity);
            if (NativeMethods.GetClassName(hWnd, name, ClassNameCapacity) == 0)
            {
                className = string.Empty;
                return false;
            }
            className = name.Length > 0 ? name.ToString() : "[NoClass]";
            return true;
        }

        private static bool TryGetWindowTitle(IntPtr hWnd, out string windowTitle)
        {
            var title = new StringBuilder(WindowTitleCapacity);
            if (NativeMethods.GetWindowText(hWnd, title, WindowTitleCapacity) == 0)
            {
                windowTitle = string.Empty;
                return false;
            }

            windowTitle = title.Length > 0 ? title.ToString() : "[NoTitle]";
            return true;
        }
    }
}