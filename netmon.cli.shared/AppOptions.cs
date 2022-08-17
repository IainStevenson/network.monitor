using netmon.core.Configuration;
using netmon.core.Data;
using System.Net;

namespace netmon.cli
{
    public class AppOptions
    {

        //TODO: Time between pings
        //TODO: PING Timeout
        public AppOptions()
        {
            OutputPath = DefaultOutputPath();
        }

        public List<IPAddress> Addresses { get; set; } = new List<IPAddress> { Defaults.DefaultMonitoringDestination };

        public TimeSpan Until { get; set; } = new(DateTimeOffset.UtcNow.AddYears(99).Ticks);

        public MonitorModes Mode { get; set; } = MonitorModes.TraceRouteThenPing;

        public string OutputPath { get; set; }

        /// <summary>
        /// --addresses=8.8.8.8,192,168,0,1,192,168,1,1 --mode=TraceRouteThenPing
        /// </summary>
        /// <param name="args"></param>
        public void FromArguments(string[] args)
        {
            if (args.Length == 0) return;

            var addressArg = args.Where(x => x.StartsWith("--addreses") || x.StartsWith("-a")).FirstOrDefault();
            if (addressArg != null)
            {
                var addresses = addressArg.Split('=', StringSplitOptions.TrimEntries ^ StringSplitOptions.RemoveEmptyEntries)[1];
                var assressitems = addresses.Split(',', StringSplitOptions.TrimEntries ^ StringSplitOptions.RemoveEmptyEntries);
                Addresses = assressitems.Select(s => IPAddress.Parse(s)).ToList();
            }

            var modeArg = args.Where(x => x.StartsWith("--mode") || x.StartsWith("-m")).FirstOrDefault();
            if (modeArg != null)
            {
                var modeValue = modeArg.Split('=', StringSplitOptions.TrimEntries ^ StringSplitOptions.RemoveEmptyEntries)[1];
                Mode = (MonitorModes)Enum.Parse(typeof(MonitorModes), modeValue, true);
            }

            var folderArg = args.Where(x => x.StartsWith("--output") || x.StartsWith("-o")).FirstOrDefault();
            if (folderArg != null)
            {
                var folderValue = folderArg.Split('=', StringSplitOptions.TrimEntries ^ StringSplitOptions.RemoveEmptyEntries)[1];
                OutputPath = folderValue;
                EnsureStorageDirectoryExits(OutputPath);
            }

        }
        public string FolderDelimiter = Environment.OSVersion.Platform == PlatformID.Unix ? "/" : "\\";
        public DirectoryInfo EnsureStorageDirectoryExits(string outputPath)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                outputPath = DefaultOutputPath();
            }
            var storageDirectory = new DirectoryInfo(outputPath);
            if (!storageDirectory.Exists)
            {
                storageDirectory.Create();
            }
            return storageDirectory;
        }

        private string DefaultOutputPath()
        {
            var commonDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            return $"{commonDataFolder}{FolderDelimiter}netmon";
        }
    }
}