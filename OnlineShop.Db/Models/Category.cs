using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OnlineShop.Db.Models
{
    public class Category
    {
        public Guid Id { get; set; }

        
        public string Name { get; set; }

        
        public string? Description { get; set; }

        public List<Product> Products { get; set; } = new List<Product>();
    }
}
