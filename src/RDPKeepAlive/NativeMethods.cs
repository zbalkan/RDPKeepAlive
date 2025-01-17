using System;
using System.Runtime.InteropServices;
using System.Text;

namespace RDPKeepAlive
{
    internal static partial class NativeMethods
    {
        #region Methods

        /// <summary>
        ///     Delegate for the EnumWindows callback.
        /// </summary>
        /// <param name="hWnd"> Handle to a window. </param>
        /// <param name="lParam"> Application-defined value. </param>
        /// <returns> True to continue enumeration; False to stop. </returns>
        internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        /// <summary>
        ///     Enumerates all top-level windows on the screen by passing their handles to a
        ///     callback function.
        /// </summary>
        /// <param name="lpEnumFunc"> Callback function. </param>
        /// <param name="lParam"> Application-defined value. </param>
        /// <returns> True if successful; otherwise, false. </returns>
        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        /// <summary>
        ///     Finds a window whose class name and window name match the specified strings.
        /// </summary>
        /// <param name="hwndParent"> Handle to the parent window. </param>
        /// <param name="hwndChildAfter"> Handle to the child window to start search after. </param>
        /// <param name="lpszClass"> Class name. </param>
        /// <param name="lpszWindow"> Window name. </param>
        /// <returns> Handle to the window if found; otherwise, IntPtr.Zero. </returns>
        [LibraryImport("user32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        internal static partial IntPtr FindWindowExW(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        /// <summary>
        ///     Retrieves the name of the class to which the specified window belongs.
        /// </summary>
        /// <param name="hWnd"> Handle to the window. </param>
        /// <param name="lpClassName"> Buffer for the class name. </param>
        /// <param name="nMaxCount"> Maximum number of characters to copy. </param>
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
        /// <returns> True if successful; otherwise, false. </returns>
        [LibraryImport("user32.dll", EntryPoint = "GetCursorPos", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetCursorPos(out POINT lpPoint);

        /// <summary>
        ///     Retrieves a handle to the foreground window (the window with which the user is
        ///     currently working).
        /// </summary>
        /// <returns> Handle to the foreground window. </returns>
        [LibraryImport("user32.dll")]
        internal static partial IntPtr GetForegroundWindow();

        /// <summary>
        ///     Retrieves the dimensions of the specified screen.
        /// </summary>
        /// <param name="smIndex"> System metric to retrieve. </param>
        /// <returns> The requested system metric. </returns>
        [LibraryImport("user32.dll")]
        internal static partial int GetSystemMetrics(SystemMetric smIndex);

        /// <summary>
        ///     Copies the text of the specified window's title bar into a buffer.
        /// </summary>
        /// <param name="hWnd"> Handle to the window. </param>
        /// <param name="lpWindowText"> Buffer for the window title. </param>
        /// <param name="nMaxCount"> Maximum number of characters to copy. </param>
        /// <returns>
        ///     The length of the string copied, not including the terminating null character.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

        /// <summary>
        ///     Synthesizes input events such as keystrokes, mouse movements, and button clicks.
        /// </summary>
        /// <param name="nInputs"> Number of input events to send. </param>
        /// <param name="pInputs"> Array of INPUT structures. </param>
        /// <param name="cbSize"> Size of an INPUT structure. </param>
        /// <returns>
        ///     The number of events that were successfully inserted into the input stream.
        /// </returns>
        [LibraryImport("user32.dll", SetLastError = true)]
        internal static partial uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

        /// <summary>
        ///     Sets the specified window as the foreground window.
        /// </summary>
        /// <param name="hWnd"> Handle to the window. </param>
        /// <returns> True if successful; otherwise, false. </returns>
        [LibraryImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetForegroundWindow(IntPtr hWnd);

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