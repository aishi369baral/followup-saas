namespace FollowUp.Api.DTO
{
    public class UpdateClientDto
    {
        public string Name { get; set; } = null!;
        public string? Company { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Notes { get; set; }
        
    }
}
