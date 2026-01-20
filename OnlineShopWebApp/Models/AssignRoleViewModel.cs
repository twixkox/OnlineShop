using System.ComponentModel.DataAnnotations;

namespace OnlineShopWebApp.Models
{
    public class AssignRoleViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

        [Display(Name = "Текущая роль")]
        public string CurrentRole { get; set; }

        [Required(ErrorMessage = "Выберите роль")]
        [Display(Name = "Новая роль")]
        public string SelectedRoleId { get; set; }

        public List<RoleViewModel> AvailableRoles { get; set; } = new();
    }
}
