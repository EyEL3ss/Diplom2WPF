using System;
using System.Collections.Generic;
using System.Text;

namespace AdminPanelElectroShop.Classes
{
    public class CartItem : BaseEntity
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public int Quantity { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual Product Product { get; set; }
        public virtual ProductVariant Variant { get; set; }
    }
}
