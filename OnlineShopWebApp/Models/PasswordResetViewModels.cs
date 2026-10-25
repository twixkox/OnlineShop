using System.ComponentModel.DataAnnotations;

namespace OnlineShopWebApp.Areas.Client.Models
{
    public class ForgotPasswordViewModel
    {
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Укажите email")]
        [EmailAddress(ErrorMessage = "Введите корректный email")]
        public string Email { get; set; } = "";
    }

    public class ResetPasswordViewModel
    {
        [Required]
        public string Email { get; set; } = "";

        [Required]
        public string Token { get; set; } = "";

        [Display(Name = "Новый пароль")]
        [Required(ErrorMessage = "Введите новый пароль")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Пароль должен быть от {2} до {1} символов")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Display(Name = "Повторите пароль")]
        [Required(ErrorMessage = "Повторите пароль")]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = "";
    }

    public class ReviewInputModel
    {
        public Guid ProductId { get; set; }

        [Range(1, 5, ErrorMessage = "Поставьте оценку от 1 до 5")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Отзыв не длиннее {1} символов")]
        public string? Text { get; set; }
    }
}
