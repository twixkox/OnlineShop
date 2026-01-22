using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace OnlineShopWebApp
{
    public class ProductViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Название продукта", Prompt = "Наименование")]
        [Required(ErrorMessage ="Не указано наименование товара")]
        [DataType(DataType.Text)]
        [StringLength(200, MinimumLength = 2, ErrorMessage ="Наименование от {2} до {1} символов")]
        public string Name { get; set; }

        [Display(Name = "Описание", Prompt ="Описание товара")]
        [Required(ErrorMessage ="Не задано описание")]
        [MaxLength(4096, ErrorMessage ="Максимальная длина описания {1} символов")]
        public string Description { get; set; }


        [Display(Name = "Стоимость", Prompt = "Цена продукта")]
        [Required(ErrorMessage ="Не указана стоимость товара")]
        [Range (0,1000000, ErrorMessage ="Диапозон цен от {1} до {2} рублей")]
        public decimal Cost { get; set; }

        [MaybeNull]
        public string PhotoPath { get; set; }
        [MaybeNull]
        public string ThumbnailsPhotoPath { get; set; }
        [MaybeNull]
        public IFormFile UploadedFile { get; set; }

    }
}

