namespace netmon.domain.Interfaces.Repositories
{
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

}