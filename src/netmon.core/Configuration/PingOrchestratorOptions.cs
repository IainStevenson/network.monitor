namespace netmon.core.Configuration
{
    public class PingOrchestratorOptions
    {
        /// <summary>
        /// The time to wait between ping executions per host, to limit traffic.
        /// </summary>
        public int MillisecondsBetweenPings { get; set; } = 5000;
    }
}
