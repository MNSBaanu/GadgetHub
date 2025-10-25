using System.ComponentModel.DataAnnotations;

namespace GadgetCentralAPI.Model
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public decimal Price { get; set; }
        
        [Required]
        public int Stock { get; set; }
        
        [Required]
        public string Category { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; }
        
        public DateTime UpdatedDate { get; set; }
    }
}
