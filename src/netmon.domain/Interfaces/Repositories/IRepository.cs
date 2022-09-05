namespace netmon.domain.Interfaces.Repositories
{
    /// <summary>
    /// A generic repository interface which provides capability assesment of what the derived type can do.
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Declare the capabilities of the repository to allow client to choose which repository to action among a selection.
        /// </summary>
        RepositoryCapabilities Capabilities { get; }
    }

}