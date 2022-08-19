namespace netmon.core.Interfaces
{
    public interface IStorageOrchestrator<T>
    {
        Task Store(T item);       
    }
}