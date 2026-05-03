using System.ComponentModel.DataAnnotations;

namespace TechWorldAPI.DTO
{
    public class ProductWriteDTO
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be non-negative")]
        public int Stock { get; set; }
        
        [Required]
        public string Category { get; set; } = string.Empty;
        
    }
}
