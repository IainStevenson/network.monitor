namespace netmon.core.Interfaces.Repositories
{
    [Flags]
    public enum RepositoryCapabilities
    {
        None = 0,
        Store,
        Retrieve,
        Delete,
    }

}