namespace netmon.domain.Interfaces.Repositories
{
    public interface IDeletionRepository<TKey, TItem>
    {
        Task DeleteAsync(TKey id);
    }
}