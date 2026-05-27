using System;
using System.Collections.Generic;
using System.Text;

namespace AdminPanelElectroShop.Classes
{
    public class ProductSpecification : BaseEntity
    {
        public int ProductId { get; set; }
        public string SpecKey { get; set; }
        public string SpecValue { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; }
    }
}
