using netmon.core.Models;

namespace netmon.core.Interfaces
{
    public interface IStorageOrchestrator<T>
    {
        Task Store(T item);

        Task MoveFilesToObjectStorage(CancellationToken cancellationToken);
    }
}