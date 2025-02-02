using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace RDPKeepAlive
{
    internal sealed partial class Monitor
    {
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
    }
}
