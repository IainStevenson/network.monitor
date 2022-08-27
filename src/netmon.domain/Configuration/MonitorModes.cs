using System.Diagnostics.CodeAnalysis;

namespace netmon.domain.Configuration
{
    public enum MonitorModes
    {
        TraceRouteContinuously,
        TraceRouteThenPingContinuously,
        PingContinuously
    }
}