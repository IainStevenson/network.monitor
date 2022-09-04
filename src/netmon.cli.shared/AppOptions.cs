using System.Text.RegularExpressions;

namespace netmon.cli
{
    public class AppOptions
    {

        public CaptureOptions Capture { get; set; } = new();
        public AnalysisOptions Analysis { get; set; } = new();
        public StorageOptions Storage { get; set; } = new();
        public ReportingOptions Reporting { get; set; } = new();

        ///// <summary>
        ///// --addresses=8.8.8.8,192,168,0,1,192,168,1,1 --mode=TraceRouteThenPing
        ///// </summary>
        ///// <param name="args"></param>
        //public void FromArguments(string[] args)
        //{
        //    if (args.Length == 0) return;

        //    var addressArg = args.Where(x => x.StartsWith("--addreses") || x.StartsWith("-a")).FirstOrDefault();
        //    if (addressArg != null)
        //    {
        //        var addresses = addressArg.Split('=', StringSplitOptions.TrimEntries ^ StringSplitOptions.RemoveEmptyEntries)[1];
        //        var assressitems = addresses.Split(',', StringSplitOptions.TrimEntries ^ StringSplitOptions.RemoveEmptyEntries);
        //        Addresses = assressitems.Select(s => IPAddress.Parse(s)).ToList();
        //    }

        //    var modeArg = args.Where(x => x.StartsWith("--mode") || x.StartsWith("-m")).FirstOrDefault();
        //    if (modeArg != null)
        //    {
        //        var modeValue = modeArg.Split('=', StringSplitOptions.TrimEntries ^ StringSplitOptions.RemoveEmptyEntries)[1];
        //        Mode = (MonitorModes)Enum.Parse(typeof(MonitorModes), modeValue, true);
        //    }

        //    var folderArg = args.Where(x => x.StartsWith("--outputPath") || x.StartsWith("-o")).FirstOrDefault();
        //    if (folderArg != null)
        //    {
        //        var folderValue = folderArg.Split('=', StringSplitOptions.TrimEntries ^ StringSplitOptions.RemoveEmptyEntries)[1];
        //        OutputPath = folderValue;
        //        EnsureStorageDirectoryExits(OutputPath);
        //    }


        //}

      
    }
}