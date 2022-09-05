namespace netmon.domain.Interfaces.Repositories
{
    /// <summary>
    /// Defines item retrieval from a repository of type <see cref="TItem"/> with an unique key of <see cref="TKey"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the item unique key.</typeparam>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    public interface IRetrieveRepository<TKey, TItem>
    {
        /// <summary>
        /// Retrieves the item specified by the <see cref="Guid"/>
        /// </summary>
        /// <param name="id">The items unique identifier.</param>
        /// <returns></returns>
        Task<TItem?> RetrieveAsync(TKey id);
    }
}