using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace OnlineShopWebApp.Areas.Admin.Models
{
    public class ChangeRole
    {
        [Display(Name = "Логин", Prompt = "Ваш логин")]
        [Required(ErrorMessage = "Не указан логин")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Введите валидный email")]
        [StringLength(30, MinimumLength = 5, ErrorMessage = "Логин должен быть от {2} до {1} символов")]
        [AllowNull]
        public string Login { get; set; }


        [Display(Name = "Роль")]
        [Required(ErrorMessage = "Не указана роль")]
        [AllowNull]
        public string Role { get; set; }


        [AllowNull]
        public List<SelectListItem> Roles { get; set; }
    }
}
