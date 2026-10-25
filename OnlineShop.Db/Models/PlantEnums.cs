namespace OnlineShop.Db.Models
{
    /// <summary>Требования растения к освещению.</summary>
    public enum LightRequirement
    {
        Sun = 0,
        PartShade = 1,
        Shade = 2
    }

    /// <summary>Потребность растения в поливе.</summary>
    public enum WateringLevel
    {
        Low = 0,
        Medium = 1,
        High = 2
    }

    public enum PaymentMethod
    {
        /// <summary>Оплата при получении</summary>
        OnDelivery = 0,
        /// <summary>Онлайн-оплата (ЮKassa)</summary>
        Online = 1
    }

    public enum PaymentStatus
    {
        NotRequired = 0,
        Pending = 1,
        Paid = 2,
        Failed = 3
    }
}
