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

        [HttpGet("overdue")]
        public IActionResult GetOverdueFollowUps()
        {
            var userId = Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var today = DateTime.UtcNow.Date;

            //getting User Plan i.e. Free / Paid
            var userPlan = _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Plan)
                .First();

            //getiing all the overdue follow-ups of the user
            var query = _context.FollowUps
                .Include(f => f.Client)
                .Where(f =>
                    f.Client.UserId == userId &&
                    f.Status == FollowUpStatus.Pending &&
                    f.NextFollowUpDate.Date < today
                )
                .OrderBy(f => f.NextFollowUpDate)
                .Select(f => new OverdueFollowUpDto
                {
                    FollowUpId = f.Id,
                    ClientName = f.Client.Name,
                    Reason = f.Reason,
                    DueDate = f.NextFollowUpDate
                });

            // modifying the query to return only 3 overdues if user plan is Free or else return the the whole list of overdues returned by the above query
            if (userPlan == UserPlan.Free)
                query = query.Take(3);

            return Ok(query.ToList());
        }

        
// GET: api/dashboard/today
    // PURPOSE:
    // - Used ONLY by dashboard
    // - Shows a PREVIEW of today's follow-ups
    // - Max 5 items to avoid UI congestion
    // - Full list is available on Follow-ups page
    [HttpGet("today")]
    public IActionResult GetTodayFollowUps()
    {
        // Extract userId from JWT
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var today = DateTime.UtcNow.Date;

        // Fetch only today's pending follow-ups
        // Include Client because dashboard shows client name
        var followUps = _context.FollowUps
            .Include(f => f.Client)
            .Where(f =>
                f.Client.UserId == userId &&
                f.Status == FollowUpStatus.Pending &&
                f.NextFollowUpDate.Date == today
            )
            .OrderBy(f => f.NextFollowUpDate)
            .Take(5) // IMPORTANT: dashboard preview limit
            .Select(f => new TodayFollowUpDto
            {
                FollowUpId = f.Id,
                ClientName = f.Client.Name,
                Reason = f.Reason,
                DueDate = f.NextFollowUpDate
            })
            .ToList();

        return Ok(followUps);
    }


        [HttpGet("calendar")]
        public IActionResult GetCalendarOverview()
        {
            var userId = Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var data = _context.FollowUps
                .Where(f => f.Client.UserId == userId)
                .GroupBy(f => f.NextFollowUpDate.Date)
                .Select(g => new CalendarDayDto
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .ToList();

            return Ok(data);
        }

        [HttpGet("streak")]
        public IActionResult GetConsistencyStreak()
        {
            var userId = Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var completedDates = _context.FollowUps
                .Where(f =>
                    f.Client.UserId == userId &&
                    f.Status == FollowUpStatus.Done
                )
                .Select(f => f.NextFollowUpDate.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            int streak = 0;
            var today = DateTime.UtcNow.Date;

            foreach (var date in completedDates)
            {
                if (date == today.AddDays(-streak))
                    streak++;
                else
                    break;
            }

            return Ok(new { streak });
        }
    }
}
