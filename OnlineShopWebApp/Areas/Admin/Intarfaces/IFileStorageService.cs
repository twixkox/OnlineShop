namespace OnlineShopWebApp.Areas.Admin.Intarfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveImageAsync(IFormFile file, string type);
        Task<string> GenerateThumbnailImageAsync(string relativePath);
        string GetUserPhotoPath();

        /// <summary>Сохраняет аватар пользователя квадратом 256×256 и возвращает относительный путь.</summary>
        Task<string> SaveAvatarAsync(IFormFile file, string userId);
    }
}