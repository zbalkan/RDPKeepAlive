using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;

namespace RDPKeepAlive
{
    internal sealed class Monitor : IEquatable<Monitor>
    {
        internal static List<Monitor> AllMonitors
        {
            get
            {
                var closure = new MonitorEnumCallback();
                var proc = new MonitorEnumProc(closure.Callback);
                _ = NativeMethods.EnumDisplayMonitors(HandleRef, IntPtr.Zero, proc, IntPtr.Zero);
                return closure.Monitors.Cast<Monitor>().ToList();
            }
        }

        internal Rect Bounds { get; }

        internal IntPtr Handle { get; }

        internal bool IsPrimary { get; }

        internal string Name { get; }

        internal Rect WorkingArea { get; }

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

        internal delegate bool MonitorEnumProc(IntPtr monitor, IntPtr hdc, IntPtr lprcMonitor, IntPtr lParam);

        public override bool Equals(object? obj)
        {
            if (obj is not Monitor) return false;
            return Handle == ((Monitor)obj).Handle;
        }

        public bool Equals(Monitor? other)
        {
            if (other is null) return false;
            return Handle == other.Handle;
        }

        public override int GetHashCode()
        {
            return Handle.GetHashCode();
        }

        public override string ToString()
        {
            return $"Name: {Name} | IsPrimary: {IsPrimary} | Bounds: {Bounds} | WorkingArea: {WorkingArea}";
        }

        internal static Rect GetMonitorBounds(IntPtr hWindow)
        {
            var monitorInfoEx = new MonitorInfoEx();
            var hMonitor = GetMonitorHandleFromWindow(hWindow);
            _ = NativeMethods.GetMonitorInfo(hMonitor, monitorInfoEx);
            return monitorInfoEx.rcMonitor;
        }

        internal static Monitor GetMonitorFromWindow(IntPtr hWnd)
        {
            var hMonitor = GetMonitorHandleFromWindow(hWnd);
            return AllMonitors.First(monitor => monitor.Handle == hMonitor.Handle);
        }

        internal static Rect GetMonitorWorkArea(IntPtr hWindow)
        {
            var monitorInfoEx = new MonitorInfoEx();
            var hMonitor = GetMonitorHandleFromWindow(hWindow);
            _ = NativeMethods.GetMonitorInfo(hMonitor, monitorInfoEx);
            return monitorInfoEx.rcWork;
        }

        private static HandleRef GetMonitorHandleFromWindow(IntPtr hWnd)
        {
            var ptrMonitor = NativeMethods.MonitorFromWindow(hWnd, NativeMethods.MonitorDefaultTo.MONITOR_DEFAULTTONEAREST);
            return new HandleRef(null, ptrMonitor);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        internal class MonitorInfoEx
        {
            [SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed", Justification = "<Pending>")]
            [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "<Pending>")]
            internal int cbSize = Marshal.SizeOf(typeof(MonitorInfoEx));

            internal Rect rcMonitor = new();

            internal Rect rcWork = new();

            internal int dwFlags = 0;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            internal char[] szDevice = new char[32];
        }

        private class MonitorEnumCallback
        {
            internal ArrayList Monitors { get; }

            internal MonitorEnumCallback()
            {
                Monitors = [];
            }

            [SuppressMessage("Redundancy", "RCS1163:Unused parameter.", Justification = "<Pending>")]
            [SuppressMessage("Major Code Smell", "S1172:Unused method parameters should be removed", Justification = "<Pending>")]
            [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "<Pending>")]
            [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
            internal bool Callback(IntPtr monitor, IntPtr hdc, IntPtr lprcMonitor, IntPtr lParam)
            {
                _ = Monitors.Add(new Monitor(monitor, hdc));
                return true;
            }
        }
    }
}