using netmon.domain.Data;
using netmon.domain.Interfaces.Repositories;
using netmon.domain.Models;
using System.Text;

namespace netmon.domain.Storage
{

    public class PingResponseModelTextSummaryRepository :
        IStorageRepository<Guid, PingResponseModel>,
        IRepository
    {
        public RepositoryCapabilities Capabilities => RepositoryCapabilities.Store;

        private readonly DirectoryInfo _storageFolder;
        private readonly string _storageSystemFolderDelimiter;

        public PingResponseModelTextSummaryRepository(DirectoryInfo storageFolder, string storageSystemFolderDelimiter)
        {
            _storageFolder = storageFolder;
            _storageSystemFolderDelimiter = storageSystemFolderDelimiter;
        }

        public async Task StoreAsync(PingResponseModel item)
        {

            await Task.Run(() =>
            {
                var summaryReport = new StringBuilder();
                var timestamp = $"{item.Start:o}";
                var rtt = item.Response?.RoundtripTime ?? -1;                
                summaryReport.Append($"{timestamp.Replace(":", "-")}\t{item.Response?.Address ?? Defaults.NullAddress}\t{item.Response?.Buffer?.Length ?? 0}\t{rtt}\t{item.Response?.Options?.Ttl ?? 0}\t{item.Id}");

                var sumaryfileName = $@"{_storageFolder.FullName}{_storageSystemFolderDelimiter}{item.Response?.Address ?? Defaults.NullAddress}-{item.Request?.Address ?? Defaults.NullAddress}-summary.txt";
                using FileStream fs = new(sumaryfileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                using StreamWriter sw = new(fs);
                sw.AutoFlush = true;
                sw.WriteLine(summaryReport.ToString());                

            });
        }
    }
}
