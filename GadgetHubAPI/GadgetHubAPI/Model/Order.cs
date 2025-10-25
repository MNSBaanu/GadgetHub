using System.ComponentModel.DataAnnotations;

namespace GadgetHubAPI.Model
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        public string OrderNumber { get; set; } = string.Empty;
        
        [Required]
        public decimal TotalAmount { get; set; }
        
        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Processing, Shipped, Delivered, Cancelled
        
        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;
        
        public DateTime? ShippedDate { get; set; }
        
        public DateTime? DeliveredDate { get; set; }
        
        public string? Notes { get; set; }
        
        // Navigation properties
        public Customer? Customer { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public List<QuotationComparison> QuotationComparisons { get; set; } = new List<QuotationComparison>();
    }
}
