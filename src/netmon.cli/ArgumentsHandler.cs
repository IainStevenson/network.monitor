using netmon.core.Configuration;
using netmon.core.Data;
using System.Net;

namespace netmon.cli
{
    public class ArgumentsHandler
    {
        public List<IPAddress> Addresses { get; private set; } = new List<IPAddress> { Defaults.DefaultMonitoringDestination };
        public bool PingOnly { get; private set; } = false;

        public TimeSpan Until { get; set; } = new(DateTimeOffset.UtcNow.AddDays(7).Ticks);


        public MonitorModes Mode { get; set; } = MonitorModes.TraceRouteThenPing;

        /// <summary>
        /// --addresses=8.8.8.8,192,168,0,1,192,168,1,1 --pingOnly=true
        /// </summary>
        /// <param name="args"></param>
        public ArgumentsHandler(string[] args)
        {
            if (args.Length == 0) return;

            var addressArg = args.Where(x => x.StartsWith("--a") || x.StartsWith("-a")).FirstOrDefault();
            if (addressArg != null)
            {
                var addresses = addressArg.Split('=', StringSplitOptions.TrimEntries ^ StringSplitOptions.RemoveEmptyEntries)[1];
                var assressitems = addresses.Split(',', StringSplitOptions.TrimEntries ^ StringSplitOptions.RemoveEmptyEntries);
                Addresses = assressitems.Select(s => IPAddress.Parse(s)).ToList();
            }
            var pingOnlyArg = args.Where(x => x.StartsWith("--p") || x.StartsWith("-p")).FirstOrDefault();
            if (pingOnlyArg != null)
            {
                var pingOnly = pingOnlyArg.Split('=', StringSplitOptions.TrimEntries ^ StringSplitOptions.RemoveEmptyEntries)[1];
                var pingOnlyValue = bool.Parse(pingOnly ?? "false");
                PingOnly = pingOnlyValue;
            }

        }


    }

}