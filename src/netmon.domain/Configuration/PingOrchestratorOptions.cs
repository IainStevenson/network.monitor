using System.Diagnostics.CodeAnalysis;

namespace netmon.domain.Configuration
{
    [ExcludeFromCodeCoverage]
    public class PingOrchestratorOptions
    {
        /// <summary>
        /// The time to wait between ping executions per host, to limit traffic.
        /// </summary>
        public int MillisecondsBetweenPings { get; set; } = 10000;
    }
}
