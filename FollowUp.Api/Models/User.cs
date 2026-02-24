using System.ComponentModel.DataAnnotations;

namespace FollowUp.Api.Models
{
    public class User
    {
        public Guid Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public UserPlan Plan { get; set; } = UserPlan.Free;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
