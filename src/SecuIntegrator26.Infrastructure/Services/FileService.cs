using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SecuIntegrator26.Core.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SecuIntegrator26.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly string _basePath;
        private readonly ILogger<FileService> _logger;

        public FileService(IConfiguration configuration, ILogger<FileService> logger)
        {
            _logger = logger;
            // Get base storage path from config or default to 'downloads' folder
            var configuredPath = configuration["Storage:BasePath"];
            _basePath = string.IsNullOrEmpty(configuredPath) 
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "downloads") 
                : configuredPath;

            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
                _logger.LogInformation("Created storage directory: {Path}", _basePath);
            }
        }

        public async Task SaveTextAsync(string relativePath, string content)
        {
            var fullPath = GetFullPath(relativePath);
            EnsureDirectoryForFile(fullPath);
            await File.WriteAllTextAsync(fullPath, content);
            _logger.LogDebug("Saved text file: {Path}", fullPath);
        }

        public async Task SaveBytesAsync(string relativePath, byte[] content)
        {
            var fullPath = GetFullPath(relativePath);
            EnsureDirectoryForFile(fullPath);
            await File.WriteAllBytesAsync(fullPath, content);
            _logger.LogDebug("Saved binary file: {Path}", fullPath);
        }

        public async Task<string?> ReadTextAsync(string relativePath)
        {
            var fullPath = GetFullPath(relativePath);
            if (!File.Exists(fullPath)) return null;
            return await File.ReadAllTextAsync(fullPath);
        }

        public bool Exists(string relativePath)
        {
            return File.Exists(GetFullPath(relativePath));
        }

        public string GetFullPath(string relativePath)
        {
            // Prevent path traversal
            var combined = Path.GetFullPath(Path.Combine(_basePath, relativePath));
            if (!combined.StartsWith(_basePath))
            {
                throw new UnauthorizedAccessException("Access outside of base path is not allowed.");
            }
            return combined;
        }

        public void EnsureDirectory(string relativePath)
        {
            var fullPath = GetFullPath(relativePath);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        private void EnsureDirectoryForFile(string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}
