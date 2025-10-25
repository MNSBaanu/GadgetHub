using System.ComponentModel.DataAnnotations;

namespace GadgetHubWeb.Models
{
    public class Customer
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = "";
        
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = "";
        
        [Required]
        [MaxLength(200)]
        public string Email { get; set; } = "";
        
        [Required]
        [MaxLength(20)]
        public string Phone { get; set; } = "";
        
        [MaxLength(500)]
        public string? Address { get; set; } = "";
        
        [MaxLength(100)]
        public string? City { get; set; } = "";
        
        [MaxLength(100)]
        public string? Country { get; set; } = "";
        
        [MaxLength(500)]
        public string? Password { get; set; }
        
        public DateTime? LastLogin { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}

