using System.Diagnostics.CodeAnalysis;

namespace netmon.core.Configuration
{
    /// <summary>
    /// Operating options for the <see cref="Orchestrators.TraceRouteOrchestrator"/>
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TraceRouteOrchestratorOptions
    {
        /// <summary>
        /// The maximum Hops allowed in a trace to the destination.
        /// </summary>
        public int MaxHops { get; set; } = 30;
        /// <summary>
        /// The Maximum attempts to ping each hops router before moving on to the next router in the chain.
        /// </summary>
        public int MaxAttempts { get; set; } = 3;
    }
}
