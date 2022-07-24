
namespace netmon.core.Interfaces
{
    public interface IStorage<T>
    {
        int Count();
        Task Store(T item);
    }
}