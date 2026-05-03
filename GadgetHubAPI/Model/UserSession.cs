using System.ComponentModel.DataAnnotations;

namespace GadgetHubAPI.Model
{
    public class UserSession
    {
        [Key]
        [MaxLength(100)]
        public string SessionId { get; set; } = string.Empty;
        
        public int CustomerId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        
        // Navigation property
        public Customer? Customer { get; set; }
    }
}
