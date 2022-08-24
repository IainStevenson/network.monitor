using netmon.core.Interfaces.Repositories;
using netmon.core.Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace netmon.core.Storage
{
    /// <summary>
    /// The single responsibiity of this class is to;
    /// store (insert or update) [upsert], retrieve,  or delete an instance of <see cref="PingResponseModel"/>, 
    /// or retieve an <see cref="IEnumerable{T}"/> of them using an instance keys or using a <see cref="Func{T, TResult}"/> query.
    /// </summary>
    public class PingResponseModelJsonRepository :
        IStorageRepository<Guid, PingResponseModel>,
        IRetrieveRepository<Guid, PingResponseModel>,
        IDeletionRepository<Guid, PingResponseModel>, IRepository, IFileSystemRepository
    {
        public RepositoryCapabilities Capabilities => RepositoryCapabilities.Store ^ RepositoryCapabilities.Retrieve ^ RepositoryCapabilities.Delete;

        private readonly DirectoryInfo _storageFolder;
        private readonly JsonSerializerSettings _settings;
        private readonly string _storageSystemFolderDelimiter;

        public PingResponseModelJsonRepository(DirectoryInfo storageFolder, JsonSerializerSettings settings, string storageSystemFolderDelimiter)
        {
            _storageFolder = storageFolder;
            _settings = settings;
            _storageSystemFolderDelimiter = storageSystemFolderDelimiter;
        }

        public Task DeleteAsync(Guid id)
        {
            var itemfileName = $@"*-{id}.json";
            var filesFound = _storageFolder.EnumerateFiles(itemfileName, SearchOption.TopDirectoryOnly);
            if (filesFound.Any())
            {
                File.Delete(filesFound.First().FullName);
            }
            return Task.FromResult(0);
        }

        [DebuggerStepThrough]
        public Task<string> GetFileDataAsync(string fullFileName)
        {
            try
            {
                return Task.FromResult(File.ReadAllText(fullFileName));
            }
            catch { } // return empty and worry about it next time.

            return Task.FromResult(String.Empty);
        }

        public Task<IEnumerable<FileInfo>> GetFileInformationAsync(string pattern)
        {
            return Task.FromResult(_storageFolder.EnumerateFiles(pattern, SearchOption.TopDirectoryOnly));
        }

        public Task<PingResponseModel?> RetrieveAsync(Guid id)
        {
            PingResponseModel? response = null;
            var itemfileName = $@"*-{id}.json";
            var filesFound = _storageFolder.EnumerateFiles(itemfileName, SearchOption.TopDirectoryOnly);
            if (filesFound.Any())
            {
                var json = File.ReadAllText(filesFound.First().FullName);
                response = JsonConvert.DeserializeObject<PingResponseModel>(json, _settings);
            }

            return Task.FromResult(response);
        }


        public Task StoreAsync(PingResponseModel item)
        {
            var data = JsonConvert.SerializeObject(item, _settings);
            var timestamp = $"{item.Start:o}";
            var itemfileName = $@"{_storageFolder.FullName}{_storageSystemFolderDelimiter}{timestamp.Replace(":", "-")}-{item.Request.Address}-{item.Id}.json";
            File.WriteAllText(itemfileName, data);
            return Task.FromResult(0);
        }

        public Task<string> DeleteFileAsync(string fullName)
        {
            if (File.Exists(fullName))
            {
                File.Delete(fullName);
                return Task.FromResult(string.Empty);
            }
            return Task.FromResult(fullName);
        }
    }
}
