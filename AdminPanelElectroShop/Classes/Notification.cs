using System;
using System.Collections.Generic;
using System.Text;

namespace AdminPanelElectroShop.Classes
{
    public class Notifications : BaseEntity
    {
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Type { get; set; } // "order", "promo", "system", "info"
        public string? Data { get; set; }
        public bool? IsRead { get; set; } = false;

        // Navigation properties
        public virtual User? User { get; set; }
    }
}
