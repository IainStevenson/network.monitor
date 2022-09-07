using netmon.domain.Configuration;
using Newtonsoft.Json;
using System.Net;

namespace netmon.cli
{
    public class MonitorOptions
    {
        [JsonIgnore]
        public string FolderDelimiter = Environment.OSVersion.Platform == PlatformID.Unix ? "/" : "\\";

        public DirectoryInfo EnsureStorageDirectoryExists(string outputPath)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                outputPath = OutputPath;
            }
            var storageDirectory = new DirectoryInfo(outputPath);
            if (!storageDirectory.Exists)
            {
                storageDirectory.Create();
            }
            return storageDirectory;
        }

        public List<string> Addresses { get; set; } = new();

        [JsonIgnore]
        public List<IPAddress> IPAddresses
        {
            get
            {
                return Addresses.Select(x => IPAddress.Parse(x)).ToList();
            }
        }

        public TimeSpan Until { get; set; } = new(DateTimeOffset.UtcNow.AddYears(99).Ticks);

        public MonitorModes Mode { get; set; } = MonitorModes.TraceRouteThenPingContinuously;

        public string OutputPath { get; set; }= $"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}{(Environment.OSVersion.Platform == PlatformID.Unix ? "/" : "\\")}netmon";

        [JsonIgnore]
        public DirectoryInfo StorageFolder => new(OutputPath);
    }
}