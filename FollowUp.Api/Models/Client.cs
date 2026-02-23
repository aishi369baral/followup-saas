namespace FollowUp.Api.Models;

public class Client
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
  
    public string Name { get; set; } = null!;                     // =null! means “Yes, it’s null right now, but trust me — it WILL be set before use.”
    public string? Company { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }                           //? means It is allowed to be null
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = null!;
    public ICollection<Followup> FollowUps { get; set; } = new List<Followup>();
}
