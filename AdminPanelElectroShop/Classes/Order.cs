using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AdminPanelElectroShop.Classes
{
    public class Order : BaseEntity
    {
        public string OrderNumber { get; set; } = string.Empty;
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string DeliveryMethod { get; set; } = string.Empty; // "pickup" or "courier"
        public string? DeliveryAddress { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? DeliveryTimeSlot { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? PaymentStatus { get; set; } = "pending";
        public string? Status { get; set; } = "new";
        public string? TrackingNumber { get; set; }
        public string? CourierComment { get; set; }
        public int? ResponsibleSellerId { get; set; }

        [NotMapped]
        public string ResponsibleSellerName => ResponsibleSeller?.FullName ?? "Не назначен";

        [NotMapped]
        public string CustomerName => User?.FullName ?? $"User #{UserId}";

        [NotMapped]
        public int ProductsCount => Items.Sum(i => i.Quantity);

        [NotMapped]
        public int PositionsCount => Items.Count;

        [NotMapped]
        public string DeliveryInfo => DeliveryMethod == "courier"
            ? DeliveryAddress ?? "Курьерская доставка"
            : "Самовывоз";

        public virtual User? User { get; set; }
        public virtual User? ResponsibleSeller { get; set; }
        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
