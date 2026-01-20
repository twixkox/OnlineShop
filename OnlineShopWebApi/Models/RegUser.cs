using System.ComponentModel.DataAnnotations;

namespace OnlineShopWebApi.Models
{
    public class RegUser
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
