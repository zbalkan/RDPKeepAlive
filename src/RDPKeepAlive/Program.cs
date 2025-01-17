﻿using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace RDPKeepAlive
{
    internal static class Program
    {
        private const string MutexName = "RDPKeepAliveMutex";

        private static readonly string[] _rdpClients = [
            "TscShellContainerClass", // MSTSC.EXE
            "WindowsForms10.Window.8.app.0.1d2098a_r8_ad1" // Sysinternals RDCMAN
            ];

        private static readonly string[] _verboseFlags = ["-v", "--verbose", "/v"];

        private static bool _found;

        private static Mutex? _mutex;

        private static volatile bool _shouldStop;

        private static bool _verbose;

        private const int ClassNameCapacity = 128;
        private const int WindowTitleCapacity = 128;

        private static string _rdpClientClassName = string.Empty;

        private static string _rdpClientWindowTitle = string.Empty;

        public static void Main(string[] args)
        {
            if (args.Length > 0 && _verboseFlags.Contains(args[0]))
            {
                _verbose = true;
            }

            // Ensure console can display Unicode characters
            Console.OutputEncoding = Encoding.UTF8;

            // Display startup messages
            Console.WriteLine("RDPKeepAlive - Zafer Balkan, (c) 2025");
            Console.WriteLine("Simulating RDP activity.");
            Console.WriteLine("Press any key to stop...\n");

            // Single Instance Enforcement
            _mutex = new Mutex(true, MutexName, out bool createdNew);
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

            // Main Loop: Enumerate windows and simulate activity Loop is terminated by the
            // interrupt thread The for loop inside provides the near-60-second cycles
            while (!_shouldStop)
            {
                // This value is set every 60 seconds.
                var previousValue = false;
                // Check for RDP client windows every second
                for (var i = 0; i < 60; i++)
                {
                    if (_shouldStop)
                        break;

                    CheckRDPClientExistence();

                    if (!_found)
                    {
                        Console.WriteLine("No RDP client found. Exiting...");
                        _shouldStop = true;
                        break;
                    }

                    if (previousValue)
                    { // we already printed once we have found. Do nothing.
                    }
                    else
                    {
                        previousValue = _found;
                        if (_verbose)
                            Console.WriteLine($"{DateTime.Now:o} - Found RDP client.\n\t* Window title: {_rdpClientWindowTitle}\n\t* Class: {_rdpClientClassName}");

                        // Perform mouse movement simulation if RDP client exists
                        SimulateMouseMovement();
                    }

                    Thread.Sleep(1000); // Sleep for 1 second
                }
            }

            // Cleanup: Release and dispose the mutex
            _mutex.ReleaseMutex();
            _mutex.Dispose();
            Console.WriteLine("RDPKeepAlive terminated gracefully.");
        }

        private static void CheckRDPClientExistence()
        {
            _found = false; // Reset the flag
            if (!NativeMethods.EnumWindows(EnumRDPWindowsProc, IntPtr.Zero))
            {
                Console.WriteLine("ERROR: EnumWindows returned false!");
                Console.WriteLine(GetErrorMessage());
            }
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
            // Initialize StringBuilders for class name and window title
            var className = new StringBuilder(ClassNameCapacity);
            var windowTitle = new StringBuilder(WindowTitleCapacity);

            // Retrieve the class name of the window
            if (NativeMethods.GetClassName(hWnd, className, ClassNameCapacity) == 0)
            {
                return false; // Stop enumeration on error
            }

            // Retrieve the window title
            if (NativeMethods.GetWindowText(hWnd, windowTitle, WindowTitleCapacity) == 0)
            {
                return true;
            }

            // Handle empty class name or window title
            var clsName = className.Length > 0 ? className.ToString() : "[NoClass]";

            if (_rdpClients.Contains(clsName))
            {
                _found = true;
                _rdpClientClassName = clsName;
                _rdpClientWindowTitle = windowTitle.Length > 0 ? windowTitle.ToString() : "[NoTitle]";
            }

            return true; // Continue enumeration
        }

        private static void SimulateMouseMovement()
        {
            // Find the specific RDP window
            var windowHandle = NativeMethods.FindWindowExW(IntPtr.Zero, IntPtr.Zero, _rdpClientClassName, _rdpClientWindowTitle);
            if (windowHandle != IntPtr.Zero)
            {
                if(!TryGetMouseMovementParams(out var input))
                {
                    Console.WriteLine("ERROR: TryGetMouseMovementParams returned false!");
                    Console.WriteLine(GetErrorMessage());
                    return;
                }

                // Store the original foreground window to restore later
                var originalForegroundWindow = NativeMethods.GetForegroundWindow();
                var clientIsNotTopmost = originalForegroundWindow != windowHandle;
                if (clientIsNotTopmost)
                {
                    var originalWindowTitle = new StringBuilder(WindowTitleCapacity);
                    if (NativeMethods.GetWindowText(originalForegroundWindow, originalWindowTitle, WindowTitleCapacity) != 0 && _verbose)
                    {
                        Console.WriteLine($"{DateTime.Now:o} - Original foreground window: {originalWindowTitle}");
                    }
                    // Bring the RDP window to the foreground
                    NativeMethods.SetForegroundWindow(windowHandle);
                }

                // Send the mouse movement input
                if (NativeMethods.SendInput(1, ref input, Marshal.SizeOf(typeof(NativeMethods.INPUT))) == 0)
                {
                    Console.WriteLine("ERROR: SendInput failed!");
                    Console.WriteLine(GetErrorMessage());
                }
                else
                {
                    if (_verbose)
                        Console.WriteLine($"{DateTime.Now:o} - Mouse movement sent successfully.");
                }

                if (clientIsNotTopmost)
                {
                    // Restore the original foreground window
                    NativeMethods.SetForegroundWindow(originalForegroundWindow);
                    if (_verbose)
                        Console.WriteLine($"{DateTime.Now:o} - Restored original foreground window.");
                }
            }
        }

        private static string GetErrorMessage()
        {
            var win32Exception = new Win32Exception(Marshal.GetLastWin32Error());
            return win32Exception != null ? win32Exception.Message : "Unknown Error";
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

        /// <summary>
        ///     Thread method that waits for any keypress to signal program termination.
        /// </summary>
        private static void Interrupt()
        {
            Console.ReadKey(true); // Wait for any keypress without echoing
            Console.WriteLine("\nExiting...");
            _shouldStop = true; // Signal main loop to terminate
        }
    }
}