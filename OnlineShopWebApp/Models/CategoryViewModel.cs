using OnlineShop.Db.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineShopWebApp.Models
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
    }
}
