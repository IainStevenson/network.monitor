using netmon.core.Data;
using netmon.core.Interfaces;
using netmon.core.Models;
using System.Text;

namespace netmon.core.Storage
{

    public class PingResponseModelTextSummaryStorage : IStorage<PingResponseModel>
    {
        private readonly DirectoryInfo _storageFolder;
        private readonly string _storageSystemFolderDelimiter;
        public PingResponseModelTextSummaryStorage(DirectoryInfo storageFolder, string storageSystemFolderDelimiter)
        {
            _storageFolder = storageFolder;
            _storageSystemFolderDelimiter= storageSystemFolderDelimiter;
        }

        public int Count()
        {
            return _storageFolder.EnumerateFiles("*-summary.txt", SearchOption.TopDirectoryOnly).Count();
        }

        public async Task Store(PingResponseModel item)
        {

            var timestamp = $"{item.Start:o}";

            var sumaryfileName = $@"{_storageFolder.FullName}{_storageSystemFolderDelimiter}{item.Response?.Address?? Defaults.NullAddress}-{item.Request?.Address ?? Defaults.NullAddress}-summary.txt";
            await Task.Run(() =>
            {
                var rtt = item.Response?.RoundtripTime ?? -1;
                var summaryReport = new StringBuilder();

                summaryReport.Append($"{timestamp.Replace(":", "-")}\t{item.Response?.Address?? Defaults.NullAddress}\t{item.Response?.Buffer?.Length ?? 0}\t{rtt}\t{item.Response?.Options?.Ttl ?? 0}");
                
                using FileStream fs = new(sumaryfileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                using StreamWriter sw = new(fs);
                sw.WriteLine(summaryReport.ToString());

            });
        }
    }

}
