namespace FollowUp.Api.DTO
{
    public class CreateFollowUpDto
    {
        public Guid ClientId { get; set; }
        public DateTime NextFollowUpDate { get; set; }
        public string Reason { get; set; }

    }
}
