using Microsoft.Extensions.Diagnostics.HealthChecks;
using OnlineShop.Db.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace OnlineShopWebApp.Areas.Client.Models
{
    public class CategoryViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "Название категории")]
        [DataType(DataType.Text)]
        [StringLength(25, MinimumLength = 2, ErrorMessage = "Название должно быть не более {1} символов")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Не задано описание категории")]
        [Display(Name = "Описание категории")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Описание категории должно быть от {2} до {1} символов")]
        public string? Description { get; set; }

        [Display(Name = "Родительская категория")]
        public Guid? ParentCategoryId { get; set; }

        public string? IdentityUrl { get; set; }

        [MaybeNull]
        public string PhotoPath { get; set; }

        [MaybeNull]
        public IFormFile UploadedFile { get; set; }
    }
}
