using System.ComponentModel.DataAnnotations;

namespace OnlineShopWebApi.Models
{
    public class AuthUser
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
