using System;
using System.Runtime.InteropServices;
using System.Text;

namespace RDPKeepAlive
{
    internal static partial class NativeMethods
    {
        #region Delegates

        /// <summary>
        ///     Delegate for the EnumWindows callback.
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
        internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        #endregion Delegates

        #region Methods

        /// <summary>
        ///     Enumerates all top-level windows on the screen by passing their handles to a
        ///     callback function.
        /// </summary>
        /// <param name="lpEnumFunc">
        ///     Callback function.
        /// </param>
        /// <param name="lParam">
        ///     Application-defined value.
        /// </param>
        /// <returns>
        ///     True if successful; otherwise, false.
        /// </returns>
        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        /// <summary>
        ///     Finds a window whose class name and window name match the specified strings.
        /// </summary>
        /// <param name="hwndParent">
        ///     Handle to the parent window.
        /// </param>
        /// <param name="hwndChildAfter">
        ///     Handle to the child window to start search after.
        /// </param>
        /// <param name="lpszClass">
        ///     Class name.
        /// </param>
        /// <param name="lpszWindow">
        ///     Window name.
        /// </param>
        /// <returns>
        ///     Handle to the window if found; otherwise, IntPtr.Zero.
        /// </returns>
        [LibraryImport("user32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        internal static partial IntPtr FindWindowExW(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        /// <summary>
        ///     Retrieves the name of the class to which the specified window belongs.
        /// </summary>
        /// <param name="hWnd">
        ///     Handle to the window.
        /// </param>
        /// <param name="lpClassName">
        ///     Buffer for the class name.
        /// </param>
        /// <param name="nMaxCount">
        ///     Maximum number of characters to copy.
        /// </param>
        /// <returns>
        ///     The number of characters copied, not including the terminating null character.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        /// <summary>
        ///     Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <param name="lpPoint">
        ///     Pointer to a POINT structure that receives the screen coordinates of the cursor.
        /// </param>
        /// <returns>
        ///     True if successful; otherwise, false.
        /// </returns>
        [LibraryImport("user32.dll", EntryPoint = "GetCursorPos", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetCursorPos(out POINT lpPoint);

        /// <summary>
        ///     Retrieves a handle to the foreground window (the window with which the user is
        ///     currently working).
        /// </summary>
        /// <returns>
        ///     Handle to the foreground window.
        /// </returns>
        [LibraryImport("user32.dll")]
        internal static partial IntPtr GetForegroundWindow();

        /// <summary>
        ///     Retrieves the dimensions of the specified screen.
        /// </summary>
        /// <param name="smIndex">
        ///     System metric to retrieve.
        /// </param>
        /// <returns>
        ///     The requested system metric.
        /// </returns>
        [LibraryImport("user32.dll")]
        internal static partial int GetSystemMetrics(SystemMetric smIndex);

        /// <summary>
        ///     Retrieves a handle to a window that has the specified relationship (Z-Order or
        ///     owner) to the specified window.
        /// </summary>
        /// <param name="hWnd">
        ///     A handle to a window. The window handle retrieved is relative to this window, based
        ///     on the value of the uCmd parameter.
        /// </param>
        /// <param name="uCmd">
        ///     The relationship between the specified window and the window whose handle is to be
        ///     retrieved. This parameter can be one of the following values.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is a window handle. If no window exists
        ///     with the specified relationship to the specified window, the return value is NULL.
        ///     To get extended error information, call GetLastError.
        /// </returns>
        [LibraryImport("user32.dll", SetLastError = true)]
        internal static partial IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        /// <summary>
        ///     Copies the text of the specified window's title bar into a buffer.
        /// </summary>
        /// <param name="hWnd">
        ///     Handle to the window.
        /// </param>
        /// <param name="lpWindowText">
        ///     Buffer for the window title.
        /// </param>
        /// <param name="nMaxCount">
        ///     Maximum number of characters to copy.
        /// </param>
        /// <returns>
        ///     The length of the string copied, not including the terminating null character.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

        /// <summary>
        ///     Retrieves the identifier of the thread that created the specified window and,
        ///     optionally, the identifier of the process that created the window.
        /// </summary>
        /// <param name="hWnd">
        ///     A handle to the window.
        /// </param>
        /// <param name="lpdwProcessId">
        ///     A pointer to a variable that receives the process identifier. If this parameter is
        ///     not NULL, GetWindowThreadProcessId copies the identifier of the process to the
        ///     variable; otherwise, it does not. If the function fails, the value of the variable
        ///     is unchanged.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is the identifier of the thread that
        ///     created the window. If the window handle is invalid, the return value is zero. To
        ///     get extended error information, call GetLastError.
        /// </returns>
        [LibraryImport("user32.dll", SetLastError = true)]
        internal static partial uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /// <summary>
        ///     Synthesizes input events such as keystrokes, mouse movements, and button clicks.
        /// </summary>
        /// <param name="nInputs">
        ///     Number of input events to send.
        /// </param>
        /// <param name="pInputs">
        ///     Array of INPUT structures.
        /// </param>
        /// <param name="cbSize">
        ///     Size of an INPUT structure.
        /// </param>
        /// <returns>
        ///     The number of events that were successfully inserted into the input stream.
        /// </returns>
        [LibraryImport("user32.dll", SetLastError = true)]
        internal static partial uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

        /// <summary>
        ///     Sets the specified window as the foreground window.
        /// </summary>
        /// <param name="hWnd">
        ///     Handle to the window.
        /// </param>
        /// <returns>
        ///     True if successful; otherwise, false.
        /// </returns>
        [LibraryImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        ///     Changes the size, position, and Z order of a child, pop-up, or top-level window.
        ///     These windows are ordered according to their appearance on the screen. The topmost
        ///     window receives the highest rank and is the first window in the Z order.
        /// </summary>
        /// <param name="hWnd">
        ///     A handle to the window.
        /// </param>
        /// <param name="hWndInsertAfter">
        ///     A handle to the window to precede the positioned window in the Z order. This
        ///     parameter must be a window handle or one of the following values.
        /// </param>
        /// <param name="X">
        ///     The new position of the left side of the window, in client coordinates.
        /// </param>
        /// <param name="Y">
        ///     The new position of the top of the window, in client coordinates.
        /// </param>
        /// <param name="cx">
        ///     The new width of the window, in pixels.
        /// </param>
        /// <param name="cy">
        ///     The new height of the window, in pixels.
        /// </param>
        /// <param name="uFlags">
        ///     The window sizing and positioning flags. This parameter can be a combination of the
        ///     following values.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is nonzero. If the function fails, the
        ///     return value is zero.To get extended error information, call GetLastError.
        /// </returns>
        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        #endregion Methods

        #region Data Structures

        /// <summary>
        ///     Specifies the type of input event.
        /// </summary>
        internal enum InputType : uint
        {
            INPUT_MOUSE = 0,

            // INPUT_KEYBOARD = 1, INPUT_HARDWARE = 2
        }

        /// <summary>
        ///     Specifies various mouse event flags.
        /// </summary>
        [Flags]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1135:Declare enum member with zero value (when enum has FlagsAttribute)", Justification = "0 is not defined in the specs")]
        internal enum MouseEventFlags : uint
        {
            MOVE = 0x0001,          // Mouse move

            LEFTDOWN = 0x0002,      // Left button down

            LEFTUP = 0x0004,        // Left button up

            RIGHTDOWN = 0x0008,     // Right button down

            RIGHTUP = 0x0010,       // Right button up

            MIDDLEDOWN = 0x0020,    // Middle button down

            MIDDLEUP = 0x0040,      // Middle button up

            XDOWN = 0x0080,         // X button down

            XUP = 0x0100,           // X button up

            WHEEL = 0x0800,         // Mouse wheel

            HWHEEL = 0x01000,       // Horizontal wheel

            MOVE_NOCOALESCE = 0x2000, // No coalescing

            VIRTUALDESK = 0x4000,    // Map to entire virtual desktop

            ABSOLUTE = 0x8000        // Absolute positioning
        }

        /// <summary>
        ///     The window sizing and positioning flags.
        /// </summary>
        [Flags]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1135:Declare enum member with zero value (when enum has FlagsAttribute)", Justification = "Not defined in specs")]
        internal enum SetWindowPosFlags : uint
        {
            NoMove = 0x0002,

            NoSize = 0x0001,

            NoActivate = 0x0010
        }

        /// <summary>
        ///     Specifies the system metrics to be retrieved.
        /// </summary>
        internal enum SystemMetric
        {
            SM_CXSCREEN = 0, // Width of the screen

            SM_CYSCREEN = 1, // Height of the screen

            // Additional metrics can be added here if needed
        }

        /// <summary>
        ///     Represents a generic input event.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct INPUT
        {
            public InputType type; // Type of input event

            public InputUnion U;    // Input data
        }

        /// <summary>
        ///     Represents a union of input data types.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        internal struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;

            // Keyboard and Hardware inputs can be added here if needed
        }

        /// <summary>
        ///     Represents the mouse input data.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            public int dx; // Mouse movement along the X-axis

            public int dy; // Mouse movement along the Y-axis

            public uint mouseData; // Additional data

            public MouseEventFlags dwFlags; // Mouse event flags

            public uint time; // Time stamp for the event

            public UIntPtr dwExtraInfo; // Additional data
        }

        /// <summary>
        ///     Represents a point in 2D space.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            public int X;

            public int Y;
        }

        #endregion Data Structures
    }
}