using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GadgetHubWeb.Models
{
    public class UserSession
    {
        [Key]
        [StringLength(100)]
        public string SessionId { get; set; } = string.Empty;

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        // Navigation property
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;
    }
}
