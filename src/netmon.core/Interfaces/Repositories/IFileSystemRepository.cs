﻿namespace netmon.core.Interfaces.Repositories
{
    public interface IFileSystemRepository
    {
        Task<IEnumerable<FileInfo>> GetFileInformationAsync(string pattern);
        Task<string> GetFileDataAsync(string fullFileName);
        Task DeleteFileAsync(string fullFileName);
    }

}