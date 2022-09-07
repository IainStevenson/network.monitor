namespace netmon.domain.Interfaces.Repositories
{
    /// <summary>
    /// Defines deletion from a repository of items of type <see cref="TItem"/> with an unique key of <see cref="TKey"/>
    /// </summary>
    /// <typeparam name="TKey">The type of the item unique key.</typeparam>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    public interface IDeletionRepository<TKey, TItem>
    {
        Task DeleteAsync(TKey id);
    }
}