using System;

namespace AdminPanelElectroShop.Classes
{
    public class ProductDiscount : BaseEntity
    {
        public int ProductId { get; set; }
        public string DiscountType { get; set; } = "percentage";
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual Product? Product { get; set; }
    }
}
