using System;
using System.Collections.Generic;
using System.Text;

namespace AdminPanelElectroShop.Classes
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public int CategoryId { get; set; }
        public string? Description { get; set; }
        public string? ShortDescription { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string? MainImageUrl { get; set; }
        public decimal? Rating { get; set; }
        public int? ReviewsCount { get; set; }
        public bool? InStock { get; set; }
        public int? StockQuantity { get; set; }
        public bool? IsNew { get; set; }
        public bool? IsHit { get; set; }
        public int? ViewsCount { get; set; }
        public int? SalesCount { get; set; }
        public string? Status { get; set; } = "approved";

        // Navigation properties
        public virtual Category? Category { get; set; }
        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public virtual ICollection<ProductSpecification> Specifications { get; set; } = new List<ProductSpecification>();
        public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<ProductDiscount> Discounts { get; set; } = new List<ProductDiscount>();
    }
}
