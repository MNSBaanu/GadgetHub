using System.ComponentModel.DataAnnotations;

namespace GadgetHubWeb.Models
{
    public class Order
    {
        public int Id { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string OrderNumber { get; set; } = "";
        
        [Required]
        public decimal TotalAmount { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "";
        
        [MaxLength(1000)]
        public string? Notes { get; set; }
        
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Customer Customer { get; set; } = null!;
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}

