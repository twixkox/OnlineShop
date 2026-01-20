using OnlineShopWebApp.Helpers;
using System.ComponentModel.DataAnnotations;

namespace OnlineShopWebApp.Models
{
    public class DeliveryUserInfoViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Имя покупателя", Prompt = "Имя")]
        [Required(ErrorMessage = "Не указанно имя")]
        [DataType(DataType.Text)]
        [StringLength(25, MinimumLength = 2, ErrorMessage = "Имя должно быть не более {1} символов")]
        public required string UserName { get; set; }


        [Display(Name = "Телефон", Prompt = "+7(999)123-12-12")]
        [Required(ErrorMessage = "Не указан номер телефона")]
        [DataType(DataType.PhoneNumber)]
        [Phone(ErrorMessage = "Телефон не может содержать буквы")]
        [StringLength(16, MinimumLength = 5, ErrorMessage = "Телефон может содержать от {2} до {1} символов")]
        public required string Phone { get; set; }


        [Display(Name = "Адрес доставки", Prompt = "Ваш адрес")]
        [Required(ErrorMessage = "Не указан адрес доставки")]
        [DataType(DataType.Text)]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Адрес должен быть от {2} до {1} символов")]
        public required string Adress { get; set; }


        [Display(Name = "Дата доставки")]
        [Required(ErrorMessage = "Не выбрана дата доставки")]
        [DateRange]
        [DataType(DataType.Date)]
        public required DateOnly DeliveryDate { get; set; }


        [Display(Name = "Комментарий", Prompt = "Ваш комментарий")]
        [MaxLength(512, ErrorMessage = "Максимальная длина комментария {1} символов")]
        [DataType(DataType.MultilineText)]
        public string? Comment { get; set; }

    }
}
