using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Db.Models
{
    public class DeliveryUserInfo
    {
        public Guid Id { get; set; }

        public string UserName { get; set; }

        public string Phone { get; set; }

        public string Adress { get; set; }

        public string Apartment {  get; set; }

        public DateOnly DeliveryDate { get; set; }

        public string? Comment { get; set; }

    }
}
