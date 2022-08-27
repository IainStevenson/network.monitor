namespace netmon.domain.Interfaces
{
    public interface IStorageOrchestrator<T>
    {
        Task StoreAsync(T item);       
    }
}