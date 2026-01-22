using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using OnlineShopWebApp.Areas.Admin.Intarfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineShopWebApp.Models
{
    public class FileStorageService : IFileStorageService
    {
        private IWebHostEnvironment _appEnviroment;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(IWebHostEnvironment appEnviroment, ILogger<FileStorageService> logger)
        {
            _appEnviroment = appEnviroment;
            _logger = logger;
        }

        public async Task<string> SaveImageAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    throw new ArgumentException("Пустой файл");
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

                var extesnionsFile = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extesnionsFile))
                {
                    throw new ArgumentException("Выбран файл с недопустимым расширением");
                }

                var fileName = Guid.NewGuid() + extesnionsFile;

                var relativePath = Path.Combine("uploads", "products", "original", fileName);
                var physicalPath = Path.Combine(_appEnviroment.WebRootPath, relativePath);

                var directory = Path.GetDirectoryName(physicalPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation($"Файл сохранен. Путь к файлу - {physicalPath}");

                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка сохранения файла");
                return ex.Message;
            }
        }

        public async Task<string> GenerateThumbnailImageAsync(string relativePath)
        {
            var path = Path.Combine(_appEnviroment.WebRootPath, relativePath); // путь к файлу

            if (!File.Exists(path))
            {
                _logger.LogError($"Исходный файл не найден");

                throw new ArgumentException($"Исходный файл не найден");
            }

            var extensionFile = Path.GetExtension(path);// расширение файла

            var fileName = $"{Path.GetFileNameWithoutExtension(relativePath)}-thumb{extensionFile}";// объединение пути + расширения

            var thumbnailRelativePath = Path.Combine("uploads", "products", "thumbnails", fileName);

            var thumbnailPath = Path.Combine(_appEnviroment.WebRootPath, thumbnailRelativePath);

            var directory = Path.GetDirectoryName(thumbnailPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var image = await Image.LoadAsync(path))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(300, 300), // Задание параметров высоты - ширины
                    Mode = ResizeMode.Crop
                }));

                await image.SaveAsync(thumbnailPath);
            }
            return thumbnailRelativePath;
        }

        public string GetUserPhotoPath()
        {
            var fileName = "UserAvatar.png";
            var relativePath = Path.Combine("uploads", "users", "avatars", fileName);

            return relativePath;
        }
    }
}
