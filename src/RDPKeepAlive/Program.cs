using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace RDPKeepAlive
{
    internal static class Program
    {
        private static volatile bool gShouldStop; // Flag to signal termination
        private static Mutex? gMutex; // Mutex to enforce single instance

        private static readonly string[] rdpClients = [
            "TscShellContainerClass", // MSTSC.EXE
            "WindowsForms10.Window.8.app.0.1d2098a_r8_ad1" // Sysinternals RDCMAN
            ];

        private static bool verbose;
        private static bool found;
        private static readonly string[] verboseFlags = ["-v", "--verbose", "/v"];
        private const string MutexName = "RDPKeepAliveMutex"; // Unique mutex name

        private static void Main(string[] args)
        {
            if (args.Length > 0 && verboseFlags.Contains(args[0]))
            {
                verbose = true;
            }

            // Ensure console can display Unicode characters
            Console.OutputEncoding = Encoding.UTF8;

            // Display startup messages
            Console.WriteLine("RDPKeepAlive - Zafer Balkan, (c) 2025");
            Console.WriteLine("Simulating RDP activity.");
            Console.WriteLine("Press any key to stop...\n");

            // Single Instance Enforcement
            gMutex = new Mutex(true, MutexName, out bool createdNew);
            if (!createdNew)
            {
                Console.WriteLine("An instance of RDPKeepAlive is already running.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);
                return;
            }

            // Start Interrupt Thread to listen for keypress
            var interruptThread = new Thread(new ThreadStart(Interrupt))
            {
                IsBackground = true // Ensures thread doesn't prevent application exit
            };
            interruptThread.Start();

            // Main Loop: Enumerate windows and simulate activity
            while (!gShouldStop)
            {
                // Enumerate all top-level windows
                if (!NativeMethods.EnumWindows(EnumRDPWindowsProc, IntPtr.Zero))
                {
                    Console.WriteLine($"ERROR: EnumWindows returned false!");
                    Console.WriteLine(GetErrorMessage());
                }
                if (!found)
                {
                    Console.WriteLine("No RDP client found. Exiting...");
                    break;
                }

                found = false; // Reset flag for next cycle

                // Wait for 60 seconds, checking every second if termination was requested
                for (var i = 0; i < 60; i++)
                {
                    if (gShouldStop)
                        break;
                    Thread.Sleep(1000); // Sleep for 1 second
                }
            }

            // Cleanup: Release and dispose the mutex
            gMutex.ReleaseMutex();
            gMutex.Dispose();
            Console.WriteLine("RDPKeepAlive terminated gracefully.");
        }

        private static string GetErrorMessage()
        {
            var win32Exception = new Win32Exception(Marshal.GetLastWin32Error());
            return win32Exception != null ? win32Exception.Message : "Unknown Error";
        }

        /// <summary>
        ///     Thread method that waits for any keypress to signal program termination.
        /// </summary>
        private static void Interrupt()
        {
            Console.ReadKey(true); // Wait for any keypress without echoing
            Console.WriteLine("\nExiting...");
            gShouldStop = true; // Signal main loop to terminate
        }

        /// <summary>
        ///     Callback method invoked by EnumWindows for each top-level window. Identifies RDP
        ///     windows and simulates mouse movement to keep them active.
        /// </summary>
        /// <param name="hWnd"> Handle to a window. </param>
        /// <param name="lParam"> Application-defined value. </param>
        /// <returns> True to continue enumeration; False to stop. </returns>
        private static bool EnumRDPWindowsProc(IntPtr hWnd, IntPtr lParam)
        {
            const int ClassNameCapacity = 128;
            const int WindowTitleCapacity = 128;

            // Initialize StringBuilders for class name and window title
            var className = new StringBuilder(ClassNameCapacity);
            var windowTitle = new StringBuilder(WindowTitleCapacity);

            // Retrieve the class name of the window
            if (NativeMethods.GetClassName(hWnd, className, ClassNameCapacity) == 0)
            {
                Debug.WriteLine("ERROR: GetClassName failed!");
                return false; // Stop enumeration on error
            }

            // Retrieve the window title
            if (NativeMethods.GetWindowText(hWnd, windowTitle, WindowTitleCapacity) == 0)
            {
                Debug.WriteLine("No window title. Skipping...");
                return true;
            }

            // Handle empty class name or window title
            var clsName = className.Length > 0 ? className.ToString() : "[NoClass]";
            var wndTitle = windowTitle.Length > 0 ? windowTitle.ToString() : "[NoTitle]";

            if (!rdpClients.Contains(clsName))
            {
                Debug.WriteLine("Not one of the known clients. Skipping...");
                return true; // Continue enumeration
            }

            found = true;
            if (verbose)
                Console.WriteLine($"{DateTime.Now:o} - Found RDP client.\n\t* Window title: {windowTitle}\n\t* Class: {clsName}");

            // Store the original foreground window to restore later
            var originalForegroundWindow = NativeMethods.GetForegroundWindow();
            windowTitle.Clear();
            if (NativeMethods.GetWindowText(originalForegroundWindow, windowTitle, WindowTitleCapacity) != 0 && verbose)
            {
                Console.WriteLine($"{DateTime.Now:o} - Original foreground window: {windowTitle}");
            }

            // Find the specific RDP window
            var windowHandle = NativeMethods.FindWindowExW(IntPtr.Zero, IntPtr.Zero, clsName, wndTitle);
            if (windowHandle != IntPtr.Zero)
            {
                // Prepare INPUT structure for mouse movement
                var input = new NativeMethods.INPUT
                {
                    type = NativeMethods.InputType.INPUT_MOUSE,
                    U = new NativeMethods.InputUnion
                    {
                        mi = new NativeMethods.MOUSEINPUT()
                    }
                };

                // Get current cursor position
                if (!NativeMethods.GetCursorPos(out NativeMethods.POINT currentPosition))
                {
                    Console.WriteLine("ERROR: GetCursorPos failed!");
                    return true; // Continue enumeration despite the error
                }

                input = GetMouseMovement(input, currentPosition);

                // Bring the RDP window to the foreground
                NativeMethods.SetForegroundWindow(windowHandle);
                // Send the mouse movement input
                if (NativeMethods.SendInput(1, ref input, Marshal.SizeOf(typeof(NativeMethods.INPUT))) == 0)
                {
                    Console.WriteLine($"ERROR: SendInput failed!");
                    Console.WriteLine(GetErrorMessage());
                }
                else
                {
                    if (verbose)
                        Console.WriteLine($"{DateTime.Now:o} - Mouse movement sent successfully.");
                }
                // Restore the original foreground window
                NativeMethods.SetForegroundWindow(originalForegroundWindow);
                if (verbose)
                    Console.WriteLine($"{DateTime.Now:o} - Restored original foreground window.");
            }

            return true; // Continue enumeration
        }

        private static NativeMethods.INPUT GetMouseMovement(NativeMethods.INPUT input, NativeMethods.POINT currentPosition)
        {
            // Set mouse movement flags: Absolute positioning and movement
            input.U.mi.dwFlags = NativeMethods.MouseEventFlags.MOVE | NativeMethods.MouseEventFlags.ABSOLUTE;

            // Calculate normalized absolute coordinates (0 to 65535)
            var screenWidth = NativeMethods.GetSystemMetrics(NativeMethods.SystemMetric.SM_CXSCREEN);
            var screenHeight = NativeMethods.GetSystemMetrics(NativeMethods.SystemMetric.SM_CYSCREEN);

            input.U.mi.dx = (currentPosition.X * 65535) / screenWidth;
            input.U.mi.dy = (currentPosition.Y * 65535) / screenHeight;
            return input;
        }
    }
}