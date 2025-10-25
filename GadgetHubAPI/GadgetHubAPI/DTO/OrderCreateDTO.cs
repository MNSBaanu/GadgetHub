using System.ComponentModel.DataAnnotations;

namespace GadgetHubAPI.DTO
{
    public class OrderCreateDTO
    {
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        public List<OrderItemDTO> OrderItems { get; set; } = new List<OrderItemDTO>();
        
        public string? Notes { get; set; }
    }
    
    
}
