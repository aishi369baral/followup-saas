namespace FollowUp.Api.DTO
{
    public class DashboardResponseDto
    {
        public List<TodayFollowUpDto> TodayFollowUps { get; set; } = [];
        public List<OverdueFollowUpDto> OverdueFollowUps { get; set; } = [];
        public DashboardCountsDto Counts { get; set; } = null!;
        public List<CalendarFollowUpDto> Calendar { get; set; } = [];
        public int Streak { get; set; }
    }
}
