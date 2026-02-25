using FollowUp.Api.Models;

namespace FollowUp.Api.DTO
{
    public class CalendarFollowUpDto
    {
        public Guid FollowUpId { get; set; }
        public string ClientName { get; set; } = null!;
        public DateTime Date { get; set; }
        public FollowUpStatus Status { get; set; }
    }
}
