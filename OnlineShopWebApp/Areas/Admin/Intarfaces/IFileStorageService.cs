namespace OnlineShopWebApp.Areas.Admin.Intarfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveImageAsync(IFormFile file);
        Task<string> GenerateThumbnailImageAsync(string relativePath);

        string GetUserPhotoPath();
    }
}