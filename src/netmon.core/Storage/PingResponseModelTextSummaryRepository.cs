using netmon.core.Data;
using netmon.core.Interfaces;
using netmon.core.Models;
using System.Text;

namespace netmon.core.Storage
{

    public class PingResponseModelTextSummaryRepository : 
        IStorageRepository<Guid, PingResponseModel>, IRepository
    {
        private readonly DirectoryInfo _storageFolder;
        private readonly string _storageSystemFolderDelimiter;
        public PingResponseModelTextSummaryRepository(DirectoryInfo storageFolder, string storageSystemFolderDelimiter)
        {
            _storageFolder = storageFolder;
            _storageSystemFolderDelimiter= storageSystemFolderDelimiter;
        }
        public RepositoryCapabilities Capabilities =>
           RepositoryCapabilities.Store;


        public async Task StoreAsync(PingResponseModel item)
        {
            var timestamp = $"{item.Start:o}";

            var sumaryfileName = $@"{_storageFolder.FullName}{_storageSystemFolderDelimiter}{item.Response?.Address ?? Defaults.NullAddress}-{item.Request?.Address ?? Defaults.NullAddress}-summary.txt";
            await Task.Run(() =>
            {
                var rtt = item.Response?.RoundtripTime ?? -1;
                var summaryReport = new StringBuilder();

                summaryReport.Append($"{timestamp.Replace(":", "-")}\t{item.Response?.Address ?? Defaults.NullAddress}\t{item.Response?.Buffer?.Length ?? 0}\t{rtt}\t{item.Response?.Options?.Ttl ?? 0}\t{item.Id}");

                using FileStream fs = new(sumaryfileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                using StreamWriter sw = new(fs);
                sw.WriteLine(summaryReport.ToString());

            });
        }       
    }
}
