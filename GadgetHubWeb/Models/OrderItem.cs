using System.ComponentModel.DataAnnotations;

namespace GadgetHubWeb.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        
        [Required]
        public int OrderId { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        [Required]
        public decimal UnitPrice { get; set; }
        
        [Required]
        public decimal TotalPrice { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Order Order { get; set; } = null!;
    }
}

