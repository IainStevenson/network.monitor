namespace netmon.domain.Interfaces.Repositories
{
    public interface IFileSystemRepository
    {
        Task<IEnumerable<FileInfo>> GetFileInformationAsync(string pattern);
        Task<string> GetFileDataAsync(string fullFileName);
        Task<string> DeleteFileAsync(string fullFileName);
    }

}