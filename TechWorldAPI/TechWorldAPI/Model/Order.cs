using System.ComponentModel.DataAnnotations;

namespace TechWorldAPI.Model
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string OrderNumber { get; set; } = string.Empty;
        
        [Required]
        public string CustomerName { get; set; } = string.Empty;
        
        [Required]
        public string CustomerEmail { get; set; } = string.Empty;
        
        [Required]
        public string CustomerPhone { get; set; } = string.Empty;
        
        [Required]
        public string ShippingAddress { get; set; } = string.Empty;
        
        [Required]
        public decimal TotalAmount { get; set; }
        
        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Processing, Shipped, Delivered, Cancelled
        
        public DateTime OrderDate { get; set; }
        
        public DateTime? ShippedDate { get; set; }
        
        public DateTime? DeliveredDate { get; set; }
        
        public string? Notes { get; set; }
        
        // Navigation properties
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
