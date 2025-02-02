using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace RDPKeepAlive
{

    internal sealed partial class Monitor
    {
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
