using System;
using System.Collections.Generic;
using System.Text;

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

        // Navigation properties
        public virtual User? User { get; set; }
        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
