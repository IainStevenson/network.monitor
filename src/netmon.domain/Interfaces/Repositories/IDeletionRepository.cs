namespace netmon.domain.Interfaces.Repositories
{
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

}