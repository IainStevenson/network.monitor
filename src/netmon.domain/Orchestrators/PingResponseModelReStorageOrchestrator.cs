using Microsoft.Extensions.Logging;
using netmon.domain.Interfaces;
using netmon.domain.Interfaces.Repositories;
using netmon.domain.Models;
using Newtonsoft.Json;

namespace netmon.domain.Orchestrators
{
    /// <summary>
    /// Handles a variety of storage options for <see cref="PingResponseModel"/>.
    /// </summary>
    public class PingResponseModelReStorageOrchestrator : IRestorageOrchestrator<PingResponseModel>
    {
        //private readonly IEnumerable<IRepository> _repositories;
        private readonly ILogger<PingResponseModelReStorageOrchestrator> _logger;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly IStorageRepository<Guid, PingResponseModel> _objectRepository;
        private readonly IFileSystemRepository _jsonRepository;


        public PingResponseModelReStorageOrchestrator(
               IStorageRepository<Guid, PingResponseModel> objectRepository,
               IFileSystemRepository jsonRepository,
                ILogger<PingResponseModelReStorageOrchestrator> logger,
                JsonSerializerSettings jsonSerializerSettings)
        {
            _logger = logger;
            _jsonSerializerSettings = jsonSerializerSettings;
            _jsonRepository = jsonRepository;
            _objectRepository = objectRepository;
        }

        /// <summary>
        /// Move all Json files found in the configured file repository directory, to the object storage service, then delete the file.
        /// </summary>
        /// <param name="cancellationToken">The asnychronous cancellation token.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task MoveFilesToObjectStorage(CancellationToken cancellationToken)
        {

            List<FileInfo> filesFound = (await _jsonRepository.GetFileInformationAsync("*.json")).ToList();

            _logger.LogTrace("Found {count} files to process", filesFound.Count);

            foreach (var file in filesFound)
            {
                if (cancellationToken.IsCancellationRequested) break;

                var isDone = await ProcessFile(file.FullName);

                if (isDone) await Cleanup(file.FullName);
            }
        }

        private async Task<bool> ProcessFile(string fullName)
        {
            var result = false;
            _logger.LogTrace("Processing file {name} ...", fullName);
            var json = await _jsonRepository.GetFileDataAsync(fullName);
            try
            {
                var response = JsonConvert.DeserializeObject<PingResponseModel>(json, _jsonSerializerSettings);
                if (response != null)
                {
                    await _objectRepository.StoreAsync(response);
                    result = true;
                }
            }
            catch (Exception) { }

           return result;
        }

        private async Task Cleanup(string fullName)
        {
            await _jsonRepository.DeleteFileAsync(fullName);

            _logger.LogTrace("Removed file {name} ", fullName);
        }
    }
}
