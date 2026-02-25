using FollowUp.Api.Data;
using FollowUp.Api.DTO;
using FollowUp.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace FollowUp.Api.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // ✅ SINGLE PUBLIC DASHBOARD ENDPOINT
        // Frontend will call ONLY this endpoint
        // =========================================================
        [HttpGet]
        public IActionResult GetDashboard()
        {
            var userId = GetUserId();

            var response = new DashboardResponseDto
            {
                TodayFollowUps = GetTodayFollowUpsInternal(userId),
                OverdueFollowUps = GetOverdueFollowUpsInternal(userId),
                Counts = GetDashboardCountsInternal(userId),
                Calendar = GetCalendarItemsInternal(userId),
                Streak = GetStreakInternal(userId)
            };

            return Ok(response);
        }

        // =========================================================
        // 🔐 HELPER: Extract UserId from JWT
        // =========================================================
        private Guid GetUserId()
        {
            return Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );
        }

        // =========================================================
        // 📌 TODAY'S FOLLOW-UPS (MAX 5 FOR DASHBOARD)
        // =========================================================
        private List<TodayFollowUpDto> GetTodayFollowUpsInternal(Guid userId)
        {
            var today = DateTime.UtcNow.Date;

            return _context.FollowUps
                .Include(f => f.Client)
                .Where(f =>
                    f.Client.UserId == userId &&
                    f.Status == FollowUpStatus.Pending &&
                    f.NextFollowUpDate.Date == today
                )
                .OrderBy(f => f.NextFollowUpDate)
                .Take(5) // dashboard limit
                .Select(f => new TodayFollowUpDto
                {
                    FollowUpId = f.Id,
                    ClientName = f.Client.Name,
                    Reason = f.Reason,
                    DueDate = f.NextFollowUpDate
                })
                .ToList();
        }

        // =========================================================
        // ⏰ OVERDUE FOLLOW-UPS (MAX 3 FOR FREE PLAN)
        // =========================================================
        private List<OverdueFollowUpDto> GetOverdueFollowUpsInternal(Guid userId)
        {
            // 1️⃣ Get user's plan
            var userPlan = _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.Plan)
                .FirstOrDefault();

            var isFreeUser = userPlan == UserPlan.Free;

            var today = DateTime.UtcNow.Date;

            // 2️⃣ IMPORTANT: IQueryable, not IOrderedQueryable
            IQueryable<Followup> query = _context.FollowUps
                .Include(f => f.Client)
                .Where(f =>
                    f.Client.UserId == userId &&
                    f.Status == FollowUpStatus.Pending &&
                    f.NextFollowUpDate.Date < today
                );

            // 3️⃣ Order
            query = query.OrderBy(f => f.NextFollowUpDate);

            // 4️⃣ Apply plan-based limit
            if (isFreeUser)
            {
                query = query.Take(3);
            }

            // 5️⃣ Project to DTO
            return query
                .Select(f => new OverdueFollowUpDto
                {
                    FollowUpId = f.Id,
                    ClientName = f.Client.Name,
                    Reason = f.Reason,
                    DueDate = f.NextFollowUpDate
                })
                .ToList();
        }

        // =========================================================
        // 📊 DASHBOARD COUNTS
        // =========================================================
        private DashboardCountsDto GetDashboardCountsInternal(Guid userId)
        {
            var today = DateTime.UtcNow.Date;

            var totalClients = _context.Clients.Count(c => c.UserId == userId);

           

            var todayFollowUps = _context.FollowUps
                .Count(f =>
                    f.Client.UserId == userId &&
                    f.Status == FollowUpStatus.Pending &&
                    f.NextFollowUpDate.Date == today
                );

            var overdueFollowUps = _context.FollowUps
                .Count(f =>
                    f.Client.UserId == userId &&
                    f.Status == FollowUpStatus.Pending &&
                    f.NextFollowUpDate.Date < today
                );

            return new DashboardCountsDto
            {
                TotalClients = totalClients,
                TodayFollowUps = todayFollowUps,
                OverdueFollowUps = overdueFollowUps
            };
        }

        // =========================================================
        // 🗓 CALENDAR DATA (LIGHTWEIGHT)
        // =========================================================
        private List<CalendarFollowUpDto> GetCalendarItemsInternal(Guid userId)
        {
            return _context.FollowUps
                .Include(f => f.Client)
                .Where(f => f.Client.UserId == userId)
                .Select(f => new CalendarFollowUpDto
                {
                    FollowUpId = f.Id,
                    ClientName = f.Client.Name,
                    Date = f.NextFollowUpDate,
                    Status = f.Status
                })
                .ToList();
        }

        // =========================================================
        // 🔥 CONSISTENCY STREAK (SIMPLE MVP LOGIC)
        // =========================================================
        
        //How many consecutive days did the user complete atleast 1 follow-up
        private int GetStreakInternal(Guid userId)
        {
            //Gets only completed follow-ups
            var completedDates = _context.FollowUps
                .Where(f =>
                    f.Client.UserId == userId &&
                    f.Status == FollowUpStatus.Done
                )
                .Select(f => f.NextFollowUpDate.Date)        //Converts them to unique dates
                .Distinct()
                .OrderByDescending(d => d)                   // sort newest -> oldest  e.g. [2026-02-25, 2026-02-24, 2026-02-22]
                .ToList();

            int streak = 0;
            var currentDate = DateTime.UtcNow.Date;         // start checking from today

            foreach (var date in completedDates)            // If user completed something today, streak = 1 , If also yesterday → streak = 2, Stops immediately when a day is missing
            {
                if (date == currentDate)
                {
                    streak++;
                    currentDate = currentDate.AddDays(-1);
                }
                else
                {
                    break;
                }
            }

            return streak;
        }
    }
}
