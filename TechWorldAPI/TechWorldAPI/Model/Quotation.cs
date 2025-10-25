using System.ComponentModel.DataAnnotations;

namespace TechWorldAPI.Model
{
    public class Quotation
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        [Required]
        public decimal UnitPrice { get; set; }
        
        [Required]
        public int AvailableStock { get; set; }
        
        [Required]
        public int EstimatedDeliveryDays { get; set; }
        
        [Required]
        public decimal TotalPrice { get; set; }
        
        [Required]
        public string Status { get; set; } = "Pending";
        
        public DateTime CreatedDate { get; set; }
        
        public DateTime ExpiryDate { get; set; }
        
        public string? Notes { get; set; }
        
        public Product? Product { get; set; }
    }
}
