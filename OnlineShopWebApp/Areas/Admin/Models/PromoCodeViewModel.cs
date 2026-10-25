using System.ComponentModel.DataAnnotations;

namespace OnlineShopWebApp.Areas.Admin.Models
{
    public class PromoCodeViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Код", Prompt = "например, SPRING10")]
        [Required(ErrorMessage = "Укажите код")]
        [RegularExpression(@"^[A-Za-z0-9_-]{3,30}$", ErrorMessage = "Латинские буквы, цифры, «-» и «_», от 3 до 30 символов")]
        public string? Code { get; set; }

        [Display(Name = "Скидка, %")]
        [Range(1, 90, ErrorMessage = "Скидка от {1} до {2}%")]
        public int DiscountPercent { get; set; } = 10;

        [Display(Name = "Минимальная сумма заказа, ₽")]
        [Range(0, 10000000, ErrorMessage = "Сумма не может быть отрицательной")]
        public decimal MinOrderAmount { get; set; }

        [Display(Name = "Действует до (включительно)")]
        [DataType(DataType.Date)]
        public DateOnly? ValidUntil { get; set; }

        [Display(Name = "Активен")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Описание (видно только в админке)")]
        [StringLength(200)]
        public string? Description { get; set; }
    }
}
