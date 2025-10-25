using System.ComponentModel.DataAnnotations;

namespace GadgetHubAPI.Model
{
    public class QuotationComparison
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int OrderId { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        public string DistributorName { get; set; } = string.Empty; // ElectroCom, TechWorld, GadgetCentral
        
        [Required]
        public decimal UnitPrice { get; set; }
        
        [Required]
        public int AvailableStock { get; set; }
        
        [Required]
        public int EstimatedDeliveryDays { get; set; }
        
        [Required]
        public decimal TotalPrice { get; set; }
        
        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Selected, Rejected
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public string? Notes { get; set; }
        
        // Navigation properties
        public Order? Order { get; set; }
        public Product? Product { get; set; }
    }
}
