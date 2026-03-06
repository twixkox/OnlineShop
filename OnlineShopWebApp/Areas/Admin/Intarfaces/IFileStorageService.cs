using System.Runtime.CompilerServices;

namespace OnlineShopWebApp.Areas.Admin.Intarfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveImageAsync(IFormFile file,string type);
        Task<string> GenerateThumbnailImageAsync(string relativePath);

        string GetUserPhotoPath();
    }
}