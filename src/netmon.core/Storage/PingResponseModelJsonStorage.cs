using netmon.core.Interfaces;
using netmon.core.Models;
using netmon.core.Serialisation;
using Newtonsoft.Json;
using System.Text;

namespace netmon.core.Storage
{
    public class PingResponseModelJsonFileStorage : IStorage<PingResponseModel>
    {
        private readonly DirectoryInfo _storageFolder;
        private readonly JsonSerializerSettings _settings;
        private readonly string _storageSystemFolderDelimiter; 
        public PingResponseModelJsonFileStorage(DirectoryInfo storageFolder, string storageSystemFolderDelimiter)
        {
            _storageFolder = storageFolder;
            _settings = new JsonSerializerSettings();
            _settings.Converters.Add(new IPAddressConverter());
            _settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            _settings.Converters.Add(new HostAdddresAndTypeConverter());
            _settings.Formatting = Formatting.Indented;
            _storageSystemFolderDelimiter = storageSystemFolderDelimiter;
        }

        public int Count()
        {
            return _storageFolder.EnumerateFiles("*.json", SearchOption.TopDirectoryOnly).Count();
        }

        public async Task Store(PingResponseModel item)
        {
            var timestamp = $"{item.Start:o}";
            var itemfileName = $@"{_storageFolder.FullName}{_storageSystemFolderDelimiter}{timestamp.Replace(":", "-")}-{item.Request.Address}.json";
            await Task.Run(() =>
            {
                var data = JsonConvert.SerializeObject(item, _settings);
                File.WriteAllText(itemfileName, data);
            });
        }
    }

}
