using Microsoft.AspNetCore.Http;

namespace TaskManagementApi.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string subPath);
        Task DeleteFileAsync(string filePath);
        string GetRelativeDownloadUrl(string filePath);
    }
}
