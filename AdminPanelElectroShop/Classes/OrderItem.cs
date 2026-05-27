using System;
using System.Collections.Generic;
using System.Text;

namespace AdminPanelElectroShop.Classes
{
    public class OrderItem : BaseEntity
    {
        public int OrderId { get; set; }
        public int? ProductId { get; set; }
        public int? VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }

        // Navigation properties
        public virtual Order? Order { get; set; }
        public virtual Product? Product { get; set; }
        public virtual ProductVariant? Variant { get; set; }
    }
}
