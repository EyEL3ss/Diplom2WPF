using System;
using System.Collections.Generic;
using System.Text;

namespace AdminPanelElectroShop.Classes
{
    public class ProductImage : BaseEntity
    {
        public int ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int? SortOrder { get; set; }

        // Navigation properties
        public virtual Product? Product { get; set; }
    }
}
