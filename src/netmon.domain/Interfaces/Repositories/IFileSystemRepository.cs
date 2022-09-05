namespace netmon.domain.Interfaces.Repositories
{
    /// <summary>
    /// A repository that can interact with the file system.
    /// </summary>
    public interface IFileSystemRepository
    {
        Task<IEnumerable<FileInfo>> GetFileInformationAsync(string pattern);
        Task<string> GetFileDataAsync(string fullFileName);
        Task<string> DeleteFileAsync(string fullFileName);
    }

}