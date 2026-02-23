namespace FollowUp.Api.Models;

public class Followup
{
    public Guid Id { get; set; }

    public Guid ClientId { get; set; }
    public string Reason { get; set; } = null!;

    public DateTime NextFollowUpDate { get; set; }

    public FollowUpStatus Status { get; set; } = FollowUpStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Client Client { get; set; } = null!;
}
