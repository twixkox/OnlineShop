namespace OnlineShop.Db.Models
{
    public class Review
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }

        public Product? Product { get; set; }

        public string UserId { get; set; } = "";

        public string UserName { get; set; } = "";

        /// <summary>Оценка от 1 до 5</summary>
        public int Rating { get; set; }

        public string? Text { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
