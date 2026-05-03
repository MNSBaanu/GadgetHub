using System.ComponentModel.DataAnnotations;

namespace GadgetHubAPI.DTO
{
    public class QuotationRequestDTO
    {
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        public string? Notes { get; set; }
    }
}
