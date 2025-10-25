using System.ComponentModel.DataAnnotations;

namespace TechWorldAPI.DTO
{
    public class QuotationRequestDTO
    {
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
    
    public class QuotationResponseDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int AvailableStock { get; set; }
        public int EstimatedDeliveryDays { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? Notes { get; set; }
    }
}
