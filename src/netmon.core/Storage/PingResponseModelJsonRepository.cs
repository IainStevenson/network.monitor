using netmon.core.Interfaces;
using netmon.core.Models;
using Newtonsoft.Json;

namespace netmon.core.Storage
{
    public class PingResponseModelJsonRepository :
        IStorageRepository<Guid, PingResponseModel>,
        IRetrieveRepository<Guid, PingResponseModel>,
        IDeletionRepository<Guid, PingResponseModel>, IRepository, IFileSystemQuery
    {
        private readonly DirectoryInfo _storageFolder;
        private readonly JsonSerializerSettings _settings;
        private readonly string _storageSystemFolderDelimiter;

        public PingResponseModelJsonRepository(DirectoryInfo storageFolder, JsonSerializerSettings settings, string storageSystemFolderDelimiter)
        {
            _storageFolder = storageFolder;
            _settings = settings;
            _storageSystemFolderDelimiter = storageSystemFolderDelimiter;

        }

        public RepositoryCapabilities Capabilities =>
            RepositoryCapabilities.Store ^
            RepositoryCapabilities.Retrieve ^
            RepositoryCapabilities.Delete;

        public Task DeleteAsync(Guid id)
        {
            var itemfileName = $@"{_storageFolder.FullName}{_storageSystemFolderDelimiter}*-*-{id}.json";
            var filesFound = _storageFolder.EnumerateFiles(itemfileName, SearchOption.TopDirectoryOnly);
            if (filesFound.Any())
            {
                File.Delete(filesFound.First().FullName);
            }
            return Task.FromResult(0);
        }

        public Task<string> GetFileDataAsync(string fullFileName)
        {
            return Task.FromResult(File.ReadAllText(fullFileName));
        }

        public Task<IEnumerable<FileInfo>> GetFileInformationAsync(string pattern)
        {
            return Task.FromResult(_storageFolder.EnumerateFiles(pattern, SearchOption.TopDirectoryOnly));
        }

        public Task<PingResponseModel?> RetrieveAsync(Guid id)
        {
            PingResponseModel? response = null;
            var itemfileName = $@"{_storageFolder.FullName}{_storageSystemFolderDelimiter}*-*-{id}.json";
            var filesFound = _storageFolder.EnumerateFiles(itemfileName, SearchOption.TopDirectoryOnly);
            if (filesFound.Any())
            {
                response = JsonConvert.DeserializeObject<PingResponseModel>(File.ReadAllText(filesFound.First().FullName));
            }

            return Task.FromResult(response);
        }

        public Task StoreAsync(PingResponseModel item)
        {
            var timestamp = $"{item.Start:o}";
            var itemfileName = $@"{_storageFolder.FullName}{_storageSystemFolderDelimiter}{timestamp.Replace(":", "-")}-{item.Request.Address}-{item.Id}.json";
            var data = JsonConvert.SerializeObject(item, _settings);
            File.WriteAllText(itemfileName, data);
            return Task.FromResult(0);
        }

        public Task DeleteFileAsync(string fullName)
        {
            File.Delete(fullName);
            return Task.FromResult(0);
        }
    }
}
