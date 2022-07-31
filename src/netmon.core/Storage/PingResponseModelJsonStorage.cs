using netmon.core.Interfaces;
using netmon.core.Models;
using netmon.core.Serialisation;
using Newtonsoft.Json;
using System.Text;

namespace netmon.core.Storage
{
    public class PingResponseModelJsonStorage : IStorage<PingResponseModel>
    {
        private readonly DirectoryInfo _storageFolder;
        private readonly JsonSerializerSettings _settings;
        public PingResponseModelJsonStorage(DirectoryInfo storageFolder)
        {
            _storageFolder = storageFolder;
            _settings = new JsonSerializerSettings();
            _settings.Converters.Add(new IPAddressConverter());
            _settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            _settings.Converters.Add(new HostAdddresAndTypeConverter());
            _settings.Formatting = Formatting.Indented;
        }

        public int Count()
        {
            return _storageFolder.EnumerateFiles("*.json", SearchOption.TopDirectoryOnly).Count();
        }

        public async Task Store(PingResponseModel item)
        {
            var timestamp = $"{item.Start:o}";
            var itemfileName = $"{_storageFolder.FullName}\\{timestamp.Replace(":", "-")}-{item.Request.Address}.json";
            var sumaryfileName = $"{_storageFolder.FullName}\\{item.Request.Address}-summary.txt";
            await Task.Run(() =>
            {
                var data = JsonConvert.SerializeObject(item, _settings);
                File.WriteAllText(itemfileName, data);
                var rtt = item.Response?.RoundtripTime ?? -1;
                var summaryReport = new StringBuilder();

                summaryReport.Append($"{timestamp.Replace(":", "-")}\t{item.Request.Address}\t{item.Response?.Buffer?.Length ?? 0}\t{rtt}\t{item.Response?.Options?.Ttl ?? 0}\n");

                File.AppendAllText(sumaryfileName, summaryReport.ToString());
            });
        }
    }

}
