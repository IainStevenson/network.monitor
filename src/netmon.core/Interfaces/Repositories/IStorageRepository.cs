using netmon.core.Models;

namespace netmon.core.Interfaces.Repositories
{
    /// <summary>
    /// Defines storage in a repository of items of type <see cref="T"/>.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    public interface IStorageRepository<TKey, TItem> where TItem : StorageBase
    {

        //Task<bool> Exists(Func<T, bool> predicate);
        /// <summary>
        /// Stores the item specified in the underlying storage medium.
        /// </summary>
        /// <param name="item">The isntance of the type.</param>
        /// <returns>An isntance of <see cref="Task"/>.</returns>
        Task StoreAsync(TItem item);
    }
}