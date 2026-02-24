namespace FollowUp.Api.DTO;

public class OverdueFollowUpDto
{
    public Guid FollowUpId { get; set; }
    public string ClientName { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public DateTime DueDate { get; set; }
}