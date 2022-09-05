namespace netmon.cli
{
    public class AppOptions
    {

        public MonitorOptions Monitor { get; set; } = new();
        public AnalysisOptions Analysis { get; set; } = new();
        public StorageOptions Storage { get; set; } = new();
        public ReportingOptions Reporting { get; set; } = new();
    }
}