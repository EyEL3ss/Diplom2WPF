using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AdminPanelElectroShop.Classes
{
    [Table("SellerApplications")]
    public class SellerApplication
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [MaxLength(200)]
        public string StoreName { get; set; }

        [MaxLength(500)]
        public string StoreDescription { get; set; }

        [MaxLength(200)]
        public string LegalName { get; set; }

        [MaxLength(20)]
        public string INN { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        public string Documents { get; set; }

        public string Status { get; set; } = "pending";

        public int? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public string RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
