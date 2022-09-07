namespace netmon.domain.Interfaces.Repositories
{
    [Flags]
    public enum RepositoryCapabilities
    {
        Undeclared = 0,
        Store,
        Retrieve,
        Delete,
        File,
    }

}