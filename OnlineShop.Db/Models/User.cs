using Microsoft.AspNetCore.Identity;

namespace OnlineShop.Db.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime CreationDateTime {  get; set; }

        public string ProfileImage { get; set; } = "uploads/users/avatars/UserAvatar.png";

    }
}
