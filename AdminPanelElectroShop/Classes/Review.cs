using System;
using System.Collections.Generic;
using System.Text;

namespace AdminPanelElectroShop.Classes
{
    public class Review : BaseEntity
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int? OrderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string? Advantages { get; set; }
        public string? Disadvantages { get; set; }
        public bool? IsVerifiedPurchase { get; set; }
        public int? Likes { get; set; }
        public int? Dislikes { get; set; }
        public bool? Moderated { get; set; } = false;

        // Navigation properties
        public virtual User? User { get; set; }
        public virtual Product? Product { get; set; }
        public virtual Order? Order { get; set; }
    }
}
