using FollowUp.Api.Models;

namespace FollowUp.Api.DTO
{
    public class EditFollowUpDto
    {
        public string Reason { get; set; } = null!;
        public DateTime NextFollowUpDate { get; set; }
        public FollowUpStatus Status { get; set; }
    }
}
