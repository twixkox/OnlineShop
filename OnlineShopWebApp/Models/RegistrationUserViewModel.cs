using System.ComponentModel.DataAnnotations;

namespace OnlineShopWebApp.Areas.Client.Models
{
    public class RegistrationUser
    {
        [Display(Name = "Логин", Prompt = "Введите логин")]
        [Required(ErrorMessage = "Не указан логин")]
        [StringLength(35, MinimumLength = 2, ErrorMessage = "Длина логина от {2} до {1} символов!")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage ="Введите корректный e-mail")]
        public required string UserName { get; set; }

        [Display(Name = "Пароль", Prompt ="Введите пароль")]
        [Required(ErrorMessage ="Не задан пароль")]
        [StringLength(25, MinimumLength = 5, ErrorMessage = "Длина пароля от {2} до {1} символов!")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Display(Name = "Повторите пароль", Prompt = "Введите пароль")]
        [Required(ErrorMessage = "Не указан повторный пароль")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        public required string ReplayPassword { get; set; }

        [Display(Name ="Телефон",Prompt ="Введите ваш телефон")]
        [Required(ErrorMessage ="Не указан телефон")]
        [DataType(DataType.PhoneNumber)]
        [Phone(ErrorMessage ="Номер телефона может содержать только цифры")]
        [StringLength(18,MinimumLength =5,ErrorMessage = "Длина телефона от {2} до {1} символов!")]
        public required string Phone {  get; set; }

        [Display(Name ="Имя",Prompt ="Введите ваше имя")]
        [Required(ErrorMessage ="Не указано имя")]
        [DataType(DataType.Text)]
        [StringLength(25,MinimumLength =2,ErrorMessage = "Длина имени от {2} до {1} символов!")]
        public required string FirstName {  get; set; }


        [Display(Name = "Фамилия", Prompt = "Введите вашу фамилию")]
        [Required(ErrorMessage = "Не указана фамилия")]
        [DataType(DataType.Text)]
        [StringLength(25, MinimumLength = 2, ErrorMessage = "Длина фамилии от {2} до {1} символов!")]
        public required string LastName { get; set; }
    }
}
