namespace netmon.domain.Interfaces
{
    public interface IRestorageOrchestrator<T>
    {
        Task MoveFilesToObjectStorage(CancellationToken cancellationToken);
    }
}