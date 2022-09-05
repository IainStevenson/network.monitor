using netmon.domain.Models;

namespace netmon.domain.Interfaces.Repositories
{
    /// <summary>
    /// Defines storage in a repository of items of type <see cref="TItem"/> with an unique key of <see cref="TKey"/>
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