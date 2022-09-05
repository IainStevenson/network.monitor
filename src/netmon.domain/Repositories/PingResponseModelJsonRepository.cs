using Microsoft.Extensions.Logging;
using netmon.domain.Interfaces.Repositories;
using netmon.domain.Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace netmon.domain.Storage
{
    /// <summary>
    /// The single responsibiity of this class is to; provide various storage action capabilities for the <see cref="PingRequestModel"/> data type.
    /// </summary>
    public class PingResponseModelJsonRepository : IRepository,
                                                    IFileSystemRepository,
                                                    IStorageRepository<Guid, PingResponseModel>,
                                                    IRetrieveRepository<Guid, PingResponseModel>,
                                                    IDeletionRepository<Guid, PingResponseModel>
    {
        public RepositoryCapabilities Capabilities => RepositoryCapabilities.Store |
                                                        RepositoryCapabilities.Retrieve |
                                                        RepositoryCapabilities.Delete |
                                                        RepositoryCapabilities.File;

        private ILogger<PingResponseModelJsonRepository> _logger;
        private readonly DirectoryInfo _storageFolder;
        private readonly JsonSerializerSettings _settings;
        private readonly string _storageSystemFolderDelimiter;

        public PingResponseModelJsonRepository(DirectoryInfo storageFolder,
                                                JsonSerializerSettings settings,
                                                string storageSystemFolderDelimiter,
                                                ILogger<PingResponseModelJsonRepository> logger)
        {
            _storageFolder = storageFolder;
            _settings = settings;
            _storageSystemFolderDelimiter = storageSystemFolderDelimiter;
            _logger = logger;
        }

        public Task DeleteAsync(Guid id)
        {
            _logger.LogTrace("Removing item {identifier} ", id);
            var itemfileName = $@"*-{id}.json";
            var filesFound = _storageFolder.EnumerateFiles(itemfileName, SearchOption.TopDirectoryOnly);
            if (filesFound.Any())
            {
                File.Delete(filesFound.First().FullName);
            }
            return Task.FromResult(0);
        }

        /// <summary>
        /// Retrieves and returns the data within a file as text regardless of actual content type. There is no mime-type checking.
        /// Uses <see cref="DebuggerStepThroughAttribute"/> to prevent debugging stopping on any exception catches 
        /// whereby the exception is dicarded and the return value is intetnially null.
        /// </summary>
        /// <param name="fullFileName">The full file name of the file.</param>
        /// <returns>The file content if retrievable or present as text</returns>
        [DebuggerStepThrough]
        public Task<string> GetFileDataAsync(string fullFileName)
        {
            _logger.LogTrace("Retrieving file data {identifier} ", fullFileName);
            try
            {
                return Task.FromResult(File.ReadAllText(fullFileName));
            }
            catch { }

            return Task.FromResult(String.Empty);
        }

        public Task<IEnumerable<FileInfo>> GetFileInformationAsync(string pattern)
        {
            _logger.LogTrace("Retrieving file information with pattern {pattern} ", pattern);
            return Task.FromResult(_storageFolder.EnumerateFiles(pattern, SearchOption.TopDirectoryOnly));
        }

        public Task<PingResponseModel?> RetrieveAsync(Guid id)
        {
            _logger.LogTrace("Retrieving item {identifier} ", id);
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
            _logger.LogTrace("Storing item {start} {address} {response} {identifier}", item.Start, item.Request.Address.ToString(), item.Response?.RoundtripTime ?? 0, item.Id);
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
                _logger.LogTrace("Deleting file item {identifier}", fullName);
                File.Delete(fullName);
                return Task.FromResult(string.Empty);
            }
            _logger.LogTrace("File item {identifier} does not exist.", fullName);

            return Task.FromResult(fullName);
        }
    }
}
