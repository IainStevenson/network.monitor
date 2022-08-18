using Microsoft.Extensions.Logging;
using netmon.core.Interfaces;
using netmon.core.Models;
using netmon.core.Storage;
using Newtonsoft.Json;

namespace netmon.core.Orchestrators
{
    /// <summary>
    /// Handles a variety of storage options for <see cref="PingResponseModel"/>.
    /// </summary>
    public class PingResponseModelStorageOrchestrator : IStorageOrchestrator<PingResponseModel>
    {
        private readonly IEnumerable<IRepository> _repositories;
        private readonly ILogger<PingResponseModelStorageOrchestrator> _logger;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public PingResponseModelStorageOrchestrator(
                IEnumerable<IRepository> repositories,
                ILogger<PingResponseModelStorageOrchestrator> logger,
                JsonSerializerSettings jsonSerializerSettings)
        {
            _repositories = repositories;
            _logger = logger;
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        /// <summary>
        /// Store the item in all of the storage capable repositories.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Task Store(PingResponseModel item)
        {
            _logger.LogTrace("Storing item {start} {address} {response} {identifier}", item.Start, item.Request.Address.ToString(), item.Response?.RoundtripTime ?? 0, item.Id);
            var tasks = _repositories
                    .Where(w => (RepositoryCapabilities.Store | w.Capabilities) == RepositoryCapabilities.Store)
                    .Select(async (repository) => await ((IStorageRepository<Guid, PingResponseModel>)repository).StoreAsync(item)).ToArray();
            Task.WaitAll(tasks);
            return Task.FromResult(0);
        }

        public async Task MoveFilesToObjectStorage(CancellationToken cancellationToken)
        {

            if (_repositories.Where(w => w.GetType() == typeof(PingResponseModelJsonRepository)).FirstOrDefault()
                is not PingResponseModelJsonRepository jsonRepository)
                throw new InvalidOperationException("PingResponseModelJsonRepository was not configured");



            if (_repositories.Where(w => w.GetType() == typeof(PingResponseModelObjectRepository)).FirstOrDefault()
                is not PingResponseModelObjectRepository objectRepository)
                throw new InvalidOperationException("PingResponseModelObjectRepository was not configured");



            List<FileInfo> filesFound = new List<FileInfo>();
            var jsonFileRepository = jsonRepository as IFileSystemQuery;
            if (jsonFileRepository != null)
            {
                filesFound = (await jsonFileRepository.GetFileInformationAsync("*.json")).ToList();
                _logger.LogTrace("Found {count} files to process", filesFound.Count);
                foreach (var file in filesFound)
                {
                    //while (!cancellationToken.IsCancellationRequested)
                    {
                        PingResponseModel? item = null;

                        var guidValue = file.Name.Split('.').First()
                            .Split('-').Last(); // before extension, end of name, older files are not translatable to Guid so the new class will create one on deeserlialisation                    

                        try
                        {
                            if (Guid.TryParse(guidValue, out Guid fileItemId))
                            {
                                item = await jsonRepository.RetrieveAsync(fileItemId);
                            }
                            else
                            {
                                var json = await jsonFileRepository.GetFileDataAsync(file.FullName);
                                if (!string.IsNullOrEmpty(json))
                                    item = JsonConvert.DeserializeObject<PingResponseModel>(json, _jsonSerializerSettings);
                            }

                            if (item != null)
                            {

                                await objectRepository.StoreAsync(item);
                                if (fileItemId == Guid.Empty)
                                {
                                    await jsonFileRepository.DeleteFileAsync(file.FullName);
                                }
                                else
                                {
                                    await jsonRepository.DeleteAsync(fileItemId);
                                }
                                _logger.LogTrace("Processed {name} ", file.FullName);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("An exception has occured during a response file data move {name}: {message}", file.Name, ex.Message);
                        }
                    }
                    if (cancellationToken.IsCancellationRequested) break;
                }
            }
        }
    }
}
