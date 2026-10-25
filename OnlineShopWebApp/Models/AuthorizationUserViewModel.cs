using System.ComponentModel.DataAnnotations;

namespace OnlineShopWebApp.Areas.Client.Models
{
    public class AuthorizationUserViewModel
    {
        [Display(Name ="Email", Prompt ="Введите email")]
        [Required(ErrorMessage = "Не указан email")]
        [StringLength(50, MinimumLength = 2, ErrorMessage ="Длина логина от {2} до {1} символов!")]
        public required string UserName { get; set; }

        [Display(Name ="Пароль", Prompt ="Введите пароль")]
        [Required(ErrorMessage = "Не введен пароль")]
        [StringLength(25, MinimumLength = 5, ErrorMessage ="Длина пароля от {2} до {1} символов!")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
        [Display(Name ="Запомнить меня")]
        public bool RememberMe { get; set; }
    }
}
