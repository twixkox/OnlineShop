using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OnlineShopWebApp.Models
{
    public class OrderViewModel
    {
        public Guid Id { get; set; }

        [ValidateNever]
        public string UserId { get; set; }

        [ValidateNever]
        public List<CartItemViewModel> Items { get; set; }

        [Required]
        public DateTime CreationDateOrder { get; set; }
        [Required]
        public DeliveryUserInfoViewModel DeliveryUserInfo { get; set; }

        [Required]
        public OrderStatusViewModel Status { get; set; }

        public decimal? TotalCost => Items?.Sum(item => item.Cost);

        public int? ItemsQuantity => Items?.Sum(item => item.Quantity);
    }
}
