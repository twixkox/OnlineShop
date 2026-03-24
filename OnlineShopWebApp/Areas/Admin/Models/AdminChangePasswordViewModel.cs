using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace OnlineShopWebApp.Areas.Admin.Models
{
    public class AdminChangePasswordViewModel
    {
        [Required]
        public string UserId { get; set; }

        [Display(Name ="Логин",Prompt ="Ваш логин")]
        [Required(ErrorMessage = "Не указан логин")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage ="Введите корректный e-mail")]
        [StringLength(30,MinimumLength = 6,ErrorMessage ="Логин должен быть от {2} до {1} символов")]
        [AllowNull]
        public string Email {  get; set; }

        [Display(Name = "Пароль", Prompt = "Введите пароль")]
        [Required(ErrorMessage = "Не задан пароль")]
        [DataType(DataType.Password)]
        [StringLength(30, MinimumLength = 6, ErrorMessage = "Пароль должен быть от {2} до {1} символов")]
        [AllowNull]
        public string Password { get; set; }

        [Display(Name = "Пароль", Prompt = "Введите пароль")]
        [Required(ErrorMessage = "Не задан пароль")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage ="Пароли не совпадают")]
        [StringLength(30, MinimumLength = 6, ErrorMessage = "Пароль должен быть от {2} до {1} символов")]
        [AllowNull]
        public string ConfirmPassword { get; set; }
    }
}
