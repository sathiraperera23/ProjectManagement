using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using TaskManagementApi.Application.Interfaces;

namespace TaskManagementApi.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;

        public FileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string subPath)
        {
            var uploadDir = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", subPath);
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path from webroot
            return Path.Combine("uploads", subPath, fileName);
        }

        public async Task DeleteFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_environment.WebRootPath ?? "wwwroot", filePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            await Task.CompletedTask;
        }

        public string GetRelativeDownloadUrl(string filePath)
        {
            // Already a relative path in our implementation
            return $"/{filePath.Replace("\\", "/")}";
        }
    }
}
