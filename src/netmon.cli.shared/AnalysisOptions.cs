namespace netmon.cli
{
    public class AnalysisOptions
    {
        public DateTimeOffset FromTime {  get;set;} = DateTimeOffset.MinValue;
        public DateTimeOffset ToTime { get;set;} = DateTimeOffset.UtcNow;
    }
}