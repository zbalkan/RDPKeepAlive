using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace RDPKeepAlive
{
    internal sealed partial class Monitor
    {
        private static readonly HandleRef HandleRef = new(null, IntPtr.Zero);

        private Monitor(IntPtr monitor, IntPtr? hdc)
        {
            var info = new MonitorInfoEx();
            _ = NativeMethods.GetMonitorInfo(new HandleRef(null, monitor), info);
            Bounds = new Rect(
                info.rcMonitor.Left, info.rcMonitor.Top,
                info.rcMonitor.Right - info.rcMonitor.Left,
                info.rcMonitor.Bottom - info.rcMonitor.Top);
            WorkingArea = new Rect(
                info.rcWork.Left, info.rcWork.Top,
                info.rcWork.Right - info.rcWork.Left,
                info.rcWork.Bottom - info.rcWork.Top);
            IsPrimary = (info.dwFlags & (int)NativeMethods.MonitorDefaultTo.MONITOR_DEFAULTTOPRIMARY) != 0;
            Name = new string(info.szDevice).TrimEnd((char)0);
            Handle = monitor;
        }

        internal static IEnumerable<Monitor> AllMonitors
        {
            get
            {
                var closure = new MonitorEnumCallback();
                var proc = new MonitorEnumProc(closure.Callback);
                _ = NativeMethods.EnumDisplayMonitors(HandleRef, IntPtr.Zero, proc, IntPtr.Zero);
                return closure.Monitors.Cast<Monitor>();
            }
        }

        internal Rect Bounds { get; }
        internal bool IsPrimary { get; }
        internal string Name { get; }
        internal Rect WorkingArea { get; }
        internal IntPtr Handle { get; }

        internal static HandleRef GetMonitorHandleFromWindow(IntPtr hWnd)
        {
            var ptrMonitor = NativeMethods.MonitorFromWindow(hWnd, NativeMethods.MonitorDefaultTo.MONITOR_DEFAULTTONEAREST);
            return new HandleRef(null, ptrMonitor);
        }

        internal static Rect GetMonitorWorkArea(IntPtr hWindow)
        {
            var monitorInfoEx = new MonitorInfoEx();
            var hMonitor = GetMonitorHandleFromWindow(hWindow);
            _ = NativeMethods.GetMonitorInfo(hMonitor, monitorInfoEx);
            return monitorInfoEx.rcWork;
        }

        internal static Rect GetMonitorBounds(IntPtr hWindow)
        {
            var monitorInfoEx = new MonitorInfoEx();
            var hMonitor = GetMonitorHandleFromWindow(hWindow);
            _ = NativeMethods.GetMonitorInfo(hMonitor, monitorInfoEx);
            return monitorInfoEx.rcMonitor;
        }

        public override string ToString()
        {
            return $"Name: {Name} | IsPrimary: {IsPrimary} | Bounds: {Bounds} | WorkingArea: {WorkingArea}";
        }

        internal delegate bool MonitorEnumProc(IntPtr monitor, IntPtr hdc, IntPtr lprcMonitor, IntPtr lParam);
    }
}
