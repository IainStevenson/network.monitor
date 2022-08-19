namespace netmon.core.Interfaces
{
    public interface IRestorageOrchestrator<T>
    {
        Task MoveFilesToObjectStorage(CancellationToken cancellationToken);
    }
}