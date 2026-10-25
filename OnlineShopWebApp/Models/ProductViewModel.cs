using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace OnlineShopWebApp.Areas.Client.Models
{
    public class ProductViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Название продукта", Prompt = "Наименование")]
        [Required(ErrorMessage = "Не указано наименование товара")]
        [DataType(DataType.Text)]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Наименование от {2} до {1} символов")]
        public string Name { get; set; }

        [Display(Name = "Описание", Prompt = "Описание товара")]
        [Required(ErrorMessage = "Не задано описание")]
        [MaxLength(4096, ErrorMessage = "Максимальная длина описания {1} символов")]
        public string Description { get; set; }


        [Display(Name = "Стоимость", Prompt = "Цена продукта")]
        [Required(ErrorMessage = "Не указана стоимость товара")]
        [Range(0, 1000000, ErrorMessage = "Диапозон цен от {1} до {2} рублей")]
        public decimal Cost { get; set; }

        [MaybeNull]
        public string PhotoPath { get; set; }
        [MaybeNull]
        public string ThumbnailsPhotoPath { get; set; }

        [MaybeNull]
        public string CurrentCategoryName { get; set; }

        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "Выберите категорию")]
        [Display(Name = "Категория")]
        public List<CategoryViewModel> AvailableCategory { get; set; } = new();

        [MaybeNull]
        public IFormFile UploadedFile { get; set; }

        [Display(Name = "Остаток на складе")]
        [Range(0, 100000, ErrorMessage = "Остаток от {1} до {2} шт.")]
        public int Stock { get; set; } = 10;

        [Display(Name = "Высота", Prompt = "например, до 3 м")]
        [StringLength(50)]
        public string? Height { get; set; }

        [Display(Name = "Зона морозостойкости")]
        [Range(1, 11, ErrorMessage = "Зона от {1} до {2}")]
        public int? FrostZone { get; set; }

        [Display(Name = "Освещение")]
        public OnlineShop.Db.Models.LightRequirement? Light { get; set; }

        [Display(Name = "Полив")]
        public OnlineShop.Db.Models.WateringLevel? Watering { get; set; }

        [Display(Name = "Цветение / плодоношение", Prompt = "например, июнь–июль")]
        [StringLength(50)]
        public string? FloweringPeriod { get; set; }

        /// <summary>Средняя оценка по отзывам (заполняется для витрины)</summary>
        public double Rating { get; set; }

        public int ReviewsCount { get; set; }

        public bool InStock => Stock > 0;
    }
}

