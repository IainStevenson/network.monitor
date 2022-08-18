using Microsoft.Extensions.Logging;
using netmon.core.Interfaces;
using netmon.core.Models;
using netmon.core.Storage;
using Newtonsoft.Json;
using System.Text;

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
            // if (jsonFileRepository != null)
            //{
            filesFound = (await jsonFileRepository.GetFileInformationAsync("*.json")).ToList();

            _logger.LogTrace("Found {count} files to process", filesFound.Count);

            foreach (var file in filesFound)
            {

                var guidValue = GetGuidValueFromFileName(file);

                try
                {
                    var result = await GetItemFromAppropriateRepositoryAsync(guidValue, file, jsonFileRepository, jsonRepository);

                    if (result.Item1 == null) break;

                    await objectRepository.StoreAsync(result.Item1);

                    await DeleteFileAsync(result.Item2, file, jsonFileRepository, jsonRepository);


                    _logger.LogTrace("Processed {name} ", file.FullName);

                }
                catch (Exception ex)
                {
                    _logger.LogError("An exception has occured during a response file data move {name}: {message}", file.Name, ex.Message);
                }

                if (cancellationToken.IsCancellationRequested) break;
            }
        }

        private string GetGuidValueFromFileName(FileInfo file)
        {
            // 2022-08-18T12-51-24.0788798+00-00-173.231.129.65-4f6ef62f-982d-4ad1-9dbf-bfcc85c40265.json


            var identifier = new StringBuilder(file.Name.Replace(file.Extension, ""));
            
            // 2022-08-18T12-51-24.0788798+00-00-173.231.129.65-4f6ef62f-982d-4ad1-9dbf-bfcc85c40265
            // 012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            // 000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888
            identifier.Remove(0, 34);    // remove datetimeoffset-


            // 173.231.129.65-4f6ef62f-982d-4ad1-9dbf-bfcc85c40265
            // 012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            // 000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888

            var lengthOfAddress = identifier.ToString().Split('-').First().Length;
            identifier.Remove(0,lengthOfAddress + 1);

            return identifier.ToString();


           // return file.Name.Split('.').First().Split('-').Last(); // before extension, end of name, older files are not translatable to Guid so the new class will create one on deeserlialisation 
        }

        private async Task DeleteFileAsync(Guid fileItemId, FileInfo file, IFileSystemQuery jsonFileRepository, IDeletionRepository<Guid, PingResponseModel> jsonRepository)
        {
            if (fileItemId == Guid.Empty)
            {
                await jsonFileRepository.DeleteFileAsync(file.FullName);
            }
            else
            {
                await jsonRepository.DeleteAsync(fileItemId);
            }
        }

        private async Task<(PingResponseModel?, Guid)> GetItemFromAppropriateRepositoryAsync(string guidValue, 
                                                    FileInfo file, 
                                                    IFileSystemQuery jsonFileRepository, 
                                                    IRetrieveRepository<Guid,PingResponseModel> jsonRepository)
        {
            if (Guid.TryParse(guidValue, out Guid fileItemId))
            {
                return (await jsonRepository.RetrieveAsync(fileItemId), fileItemId);
            }
            else
            {
                var json = await jsonFileRepository.GetFileDataAsync(file.FullName);
                if (!string.IsNullOrEmpty(json))
                    return (JsonConvert.DeserializeObject<PingResponseModel>(json, _jsonSerializerSettings), fileItemId);
            }
            return (null, Guid.Empty);
        }
    }
}
