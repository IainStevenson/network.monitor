namespace netmon.domain.Interfaces
{
    public interface IAnalysisOrchestrator<T> where T: class
    {
        Task Execute(DateTimeOffset fromTime, DateTimeOffset toTime, CancellationToken cancellation);
    }
}
