using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AdminPanelElectroShop.Classes
{
    public class User : BaseEntity
    {
        [NotMapped]
        public string FullName => string.IsNullOrWhiteSpace(LastName)
            ? FirstName
            : $"{FirstName} {LastName}";

        public string Email { get; set; }
        public string Phone { get; set; }
        [Column("password_hash")]
        public string PasswordHash { get; set; }
        public string FirstName { get; set; } 
        public string? LastName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Role { get; set; } = "customer";
        public bool? IsActive { get; set; } = true;
        public DateTime? LastLogin { get; set; }

        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Notifications> Notifications { get; set; } = new List<Notifications>();

        public virtual ICollection<PromoCode> PromoCodes { get; set; } = new List<PromoCode>();
    }
}
