using System.ComponentModel.DataAnnotations;

namespace OnlineShopWebApp.Models
{
    public class RoleViewModel
    {
        public string Id { get; set; }


        [Required(ErrorMessage = "Не задано название роли")]
        [Display(Name  = "Название роли")]
        [StringLength(50,MinimumLength = 2, ErrorMessage = "Название роли должно быть от {2} до {1} символов")]
        public string Name { get; set; }

        [Required(ErrorMessage ="Не задано описание роли")]
        [Display(Name = "Описание роли")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Описание роли должно быть от {2} до {1} символов")]
        public string Description { get; set; }


    }
}
