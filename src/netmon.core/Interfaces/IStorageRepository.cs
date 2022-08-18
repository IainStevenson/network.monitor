
using netmon.core.Models;

namespace netmon.core.Interfaces
{
    public interface IFileSystemQuery
    {
        Task<IEnumerable<FileInfo>> GetFileInformationAsync(string pattern);
        Task<string> GetFileDataAsync(string fullFileName);
        Task DeleteFileAsync(string fullFileName);
    }

    public interface  IRepository
    {
        RepositoryCapabilities Capabilities {  get;}
    }

    [Flags]
    public enum RepositoryCapabilities
    {
        None = 0,
        Store,
        Retrieve,
        Delete,
    }

    public interface IDeletionRepository<TKey, TItem>
    {

        Task DeleteAsync(TKey id);
        /// <summary>
        /// Deletes all items matching the query
        /// </summary>
        /// <param name="query">A delegate function that identifies the items.</param>
        /// <returns>An isntance of <see cref="Task"/>.</returns>
        //Task DeleteAsync(Func<bool> query);

    }
    /// <summary>
    /// Defines item retrieval from a repository of type <see cref="T"/>
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    public interface IRetrieveRepository<TKey, TItem>
    {
        /// <summary>
        /// Retrieves the item specified by the <see cref="Guid"/>
        /// </summary>
        /// <param name="id">The items unique identifier.</param>
        /// <returns></returns>
        Task<TItem?> RetrieveAsync(TKey id);
        /// <summary>
        /// Retrieves any items matching the query.
        /// </summary>
        /// <param name="query">A delegate function that identifies the items.</param>
        /// <returns></returns>
        //Task<IEnumerable<T>> RetrieveAsync(Func<bool, T> query);
        /// <summary>
        /// Deletes the item matching the identifier.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An isntance of <see cref="Task"/>.</returns>
    }
    /// <summary>
    /// Defines storage ina repository of items of type <see cref="T"/>.
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