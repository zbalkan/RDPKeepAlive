using System;
using System.Text;
using System.Threading;

namespace RDPKeepAlive
{
    internal static class Program
    {
        private const string MutexName = "RDPKeepAliveMutex";

        private static readonly string[] _verboseFlags = ["-v", "--verbose", "/v"];

        private static bool _verbose;

        public static void Main(string[] args)
        {
            using Mutex mutex = new Mutex(false, MutexName);
            if (!mutex.WaitOne(0))
            {
                Console.WriteLine("2nd instance");
                ExitGracefully();
            }

            if (args.Length > 0 && _verboseFlags.Contains(args[0]))
            {
                _verbose = true;
            }

            // Ensure console can display Unicode characters
            Console.OutputEncoding = Encoding.UTF8;

            // Subscribe to Ctrl+C handling
            Console.CancelKeyPress += OnCancelKeyPress;

            // Display startup messages
            Console.WriteLine("RDPKeepAlive - Zafer Balkan, (c) 2025");
            Console.WriteLine("Simulating RDP activity.");
            Console.WriteLine("Press CTRL+C to stop...\n");

            // Main Loop: Enumerate windows and simulate activity Loop is terminated by the
            // interrupt thread The for loop inside provides the near-60-second cycles
            while (true)
            {
                // This value is set every 60 seconds.
                var previousValue = false;

                // Check for RDP client windows every second
                for (var i = 0; i < 60; i++)
                {
                    var isFound = KeepAlive.TryGetRDPClient(out var client);

                    if (!isFound)
                    {
                        Console.WriteLine("No RDP client found. Exiting...");
                        ExitGracefully();
                    }

                    if (previousValue)
                    { // we already printed once we have found. Do nothing.
                    }
                    else
                    {
                        previousValue = isFound;
                        if (_verbose)
                            Console.WriteLine($"{DateTime.Now:o} - Found RDP client.\n\t* Window: {client.WindowTitle}\n\t* Class : {client.ClassName}");

                        // Perform mouse movement simulation if RDP client exists
                        KeepAlive.Execute();
                    }

                    Thread.Sleep(1000); // Sleep for 1 second
                }
            }
        }

        private static void ExitGracefully()
        {
            // Add cleanup code here when needed
            Console.WriteLine("RDPKeepAlive terminated gracefully.");
            Environment.Exit(0);
        }

        private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true; // Prevent immediate termination
            ExitGracefully();
        }
    }
}