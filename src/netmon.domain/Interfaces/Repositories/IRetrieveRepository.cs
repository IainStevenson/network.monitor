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
    }
}