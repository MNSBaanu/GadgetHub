using System.ComponentModel.DataAnnotations;

namespace GadgetHubAPI.Model
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        // Price and Stock removed - now fetched dynamically from distributors
        
        [Required]
        public string Category { get; set; } = string.Empty;
              
        public DateTime CreatedDate { get; set; }
        
        public DateTime UpdatedDate { get; set; }
        
        // Navigation properties
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
