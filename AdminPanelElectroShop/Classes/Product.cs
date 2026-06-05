using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;

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
        public int? SellerId { get; set; }
        public DateTime? ReceivedAt { get; set; }
        public string? Nomenclature { get; set; }

        [NotMapped]
        public ProductDiscount? ActiveDiscount => Discounts
            .Where(d => d.IsActive && d.StartDate <= DateTime.Now && d.EndDate.Date >= DateTime.Today)
            .Select(d => new
            {
                Discount = d,
                FinalPrice = CalculateDiscountedPrice(Price, d)
            })
            .OrderBy(d => d.FinalPrice)
            .FirstOrDefault()
            ?.Discount;

        [NotMapped]
        public bool HasActiveDiscount => ActiveDiscount != null;

        [NotMapped]
        public decimal FinalPrice => ActiveDiscount == null
            ? Price
            : CalculateDiscountedPrice(Price, ActiveDiscount);

        [NotMapped]
        public string PriceText => FormatPrice(Price);

        [NotMapped]
        public string FinalPriceText => FormatPrice(FinalPrice);

        [NotMapped]
        public string DiscountText => ActiveDiscount == null
            ? string.Empty
            : ActiveDiscount.DiscountType == "percentage"
                ? $"-{ActiveDiscount.DiscountValue:N0}%"
                : $"-{FormatPrice(ActiveDiscount.DiscountValue)}";

        [NotMapped]
        public string SellerName => Seller?.FullName ?? "Не закреплен";

        [NotMapped]
        public string ReceivedAtText => ReceivedAt?.ToString("dd.MM.yyyy") ?? "Не указана";

        private static decimal CalculateDiscountedPrice(decimal price, ProductDiscount discount)
        {
            var discountedPrice = discount.DiscountType == "percentage"
                ? price - price * discount.DiscountValue / 100
                : price - discount.DiscountValue;

            return Math.Max(0, Math.Round(discountedPrice, 2));
        }

        private static string FormatPrice(decimal price)
        {
            return string.Format(CultureInfo.GetCultureInfo("ru-RU"), "{0:N0} ₽", price);
        }

        public virtual Category? Category { get; set; }
        public virtual User? Seller { get; set; }
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
