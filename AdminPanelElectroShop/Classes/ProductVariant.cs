using System;
using System.Collections.Generic;
using System.Text;

namespace AdminPanelElectroShop.Classes
{
    public class ProductVariant : BaseEntity
    {
        public int ProductId { get; set; }
        public string? Color { get; set; }
        public string? Storage { get; set; }
        public decimal? PriceAdjustment { get; set; }
        public int? StockQuantity { get; set; }

        // Navigation properties
        public virtual Product? Product { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
