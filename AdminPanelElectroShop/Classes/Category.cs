using System;
using System.Collections.Generic;
using System.Text;

namespace AdminPanelElectroShop.Classes
{
    public class Category : BaseEntity
    {
        public int? ParentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? ImageUrl { get; set; }
        public int? SortOrder { get; set; }
        public bool? IsActive { get; set; } = true;

        // Navigation properties
        public virtual Category? Parent { get; set; }
        public virtual ICollection<Category> Subcategories { get; set; } = new List<Category>();
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
