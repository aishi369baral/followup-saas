namespace FollowUp.Api.DTO
{
    public class ClientResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Company { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
