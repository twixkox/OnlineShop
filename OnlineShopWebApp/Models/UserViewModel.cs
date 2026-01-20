using Microsoft.AspNetCore.Identity;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Admin.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineShopWebApp.Models
{
    public class UserViewModel
    {
        [Display(Name = "Ваше имя", Prompt = "Введите ваше имя")]
        [Required(ErrorMessage = "Не указано имя пользователя")]
        [DataType(DataType.Text)]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Имя должно быть от {2} до {1} символов")]
        public string FirstName { get; set; }

        [Display(Name = "Фамилия", Prompt = "Введите фамилию")]
        [Required(ErrorMessage = "Не указана фамилия пользователя")]
        [DataType(DataType.Text)]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Фамилия должна быть от {2} до {1} символов")]
        public string LastName { get; set; }


        [Display(Name = "Логин", Prompt = "Логин")]
        [Required(ErrorMessage = "Не указан логин пользователя")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Введите существующий e-mail")]
        [StringLength(30, MinimumLength = 5, ErrorMessage = "Логин должен быть от {2} до {1} символов")]
        public string UserName { get; set; }


        [Display(Name = "Пароль", Prompt = "Введите ваш пароль")]
        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        [StringLength(30, MinimumLength = 8, ErrorMessage = "Пароль должен быть от {2} до {1} символов")]
        public string Password { get; set; }

        [Display(Name = "Телефон", Prompt = "Введите ваш телефон")]
        [Required(ErrorMessage = "Не указан телефон")]
        [DataType(DataType.PhoneNumber)]
        [Phone(ErrorMessage = "Номер телефона может содержать только цифры")]
        [StringLength(16, MinimumLength = 5, ErrorMessage = "Длина телефона от {2} до {1} символов!")]
        public required string Phone { get; set; }

        public string Role { get; set; }

        public string ConfirmPassword { get; set; }

        public string Id { get; set; }

        public DateTime CreationDateTime { get; set; }
    }
}
