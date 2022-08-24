namespace netmon.core.Interfaces
{
    public interface IStorageOrchestrator<T>
    {
        Task StoreAsync(T item);       
    }
}