using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace RDPKeepAlive
{
    internal static class KeepAlive
    {
        private const int ClassNameCapacity = 128;

        private const int WindowTitleCapacity = 128;

        private static readonly char[] _class = new char[ClassNameCapacity];

        private static readonly string[] _rdpClients = [
            "TscShellContainerClass",                      // MSTSC.EXE
            "WindowsForms10.Window.8.app.0.1d2098a_r8_ad1" // Sysinternals RDCMAN
            ];

        private static readonly char[] _title = new char[WindowTitleCapacity];

        private static Client _client;

        private static bool _clientIsNotTopmost;

        private static bool _found;

        private static IntPtr _originalForegroundWindow;
        private static IntPtr _windowInFront;
        /// <summary>
        ///     Executes the keep-alive process for the RDP client window. This method finds the
        ///     specific RDP window, prepares mouse movement parameters, takes a snapshot of the
        ///     current window state, processes the mouse movement to keep the RDP session active,
        ///     and then restores the original window state.
        /// </summary>
        internal static void Execute()
        {
            // Find the specific RDP window
            var clientWindow = NativeMethods.FindWindowExW(IntPtr.Zero, IntPtr.Zero, _client.ClassName, _client.WindowTitle);
            if (clientWindow == IntPtr.Zero)
            {
                return;
            }

            if (!TryGetMouseMovementParams(out var input))
            {
                throw new KeepAliveException("Failed to get mouse movement parameters.", new Win32Exception(Marshal.GetLastWin32Error()));
            }

            TakeSnapshot(clientWindow);

            ProcessMouseMovement(clientWindow, input);

            RestoreSnapshot(clientWindow);
        }

        /// <summary>
        ///     Finds the RDP client window by enumerating all top-level windows.
        /// </summary>
        /// <param name="client">
        ///     The RDP client window.
        /// </param>
        /// <returns>
        ///     True if an RDP client window is found; otherwise, false.
        /// </returns>
        internal static bool TryGetRDPClient(out Client client)
        {
            _found = false; // Reset the flag
            _ = NativeMethods.EnumWindows(EnumRDPWindowsProc, IntPtr.Zero);

            client = _client;

            return _found;
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
            if (TryGetWindowClass(hWnd, out var className) && TryGetWindowTitle(hWnd, out var windowTitle) && IsRdpClientClass(className))
            {
                _found = true;
                _client = new Client(className.ToString(), windowTitle.ToString());

                return false; // Stop enumeration
            }
            return true;
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

        private static bool IsRdpClientClass(ReadOnlySpan<char> classNameSpan)
        {
            foreach (var client in _rdpClients)
            {
                if (classNameSpan.SequenceEqual(client.AsSpan()))
                {
                    return true;
                }
            }
            return false;
        }

        private static void ProcessMouseMovement(nint clientWindow, NativeMethods.INPUT input)
        {
            if (_clientIsNotTopmost)
            {
                // Bring the RDP window to the foreground
                NativeMethods.SetForegroundWindow(clientWindow);
            }

            // Send the mouse movement input
            if (NativeMethods.SendInput(1, ref input, 40) == 0) // Marshal.SizeOf(typeof(NativeMethods.INPUT)) == 40
            {
                throw new KeepAliveException("Failed to send mouse movement input.", new Win32Exception(Marshal.GetLastWin32Error()));
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

        private static bool TryGetWindowClass(IntPtr hWnd, out ReadOnlySpan<char> className)
        {
            Array.Clear(_class, 0, ClassNameCapacity);

            if (NativeMethods.GetClassName(hWnd, _class, ClassNameCapacity) == 0)
            {
                className = default;
                return false;
            }

            var indexOfNull = Array.IndexOf(_class, char.MinValue);
            if (indexOfNull != 0)
            {
                className = _class.AsSpan(0, indexOfNull);
            }
            else
            {
                className = string.Empty;
            }

            return true;
        }

        private static bool TryGetWindowTitle(IntPtr hWnd, out ReadOnlySpan<char> windowTitle)
        {
            Array.Clear(_title, 0, WindowTitleCapacity);

            if (NativeMethods.GetWindowText(hWnd, _title, WindowTitleCapacity) == 0)
            {
                windowTitle = default;
                return false;
            }

            var indexOfNull = Array.IndexOf(_title, char.MinValue);
            if (indexOfNull != 0)
            {
                windowTitle = _title.AsSpan(0, indexOfNull);
            }
            else
            {
                windowTitle = string.Empty;
            }

            return true;
        }
    }
}