using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FollowUp.Api.Data;
using FollowUp.Api.DTO;
using FollowUp.Api.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FollowUpsController : ControllerBase
{
    private readonly AppDbContext _context;

    public FollowUpsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/followups
    [HttpGet]
    [HttpGet]
    public IActionResult GetFollowUps(
    [FromQuery] FollowUpStatus? status,
    [FromQuery] string? clientName,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20
)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 20;

        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        // Base query: only this user's follow-ups
        var query = _context.FollowUps
            .Include(f => f.Client)
            .Where(f => f.Client.UserId == userId)
            .AsQueryable();

        // Optional: filter by status
        if (status.HasValue)
        {
            query = query.Where(f => f.Status == status.Value);
        }

        // Optional: filter by client name (case-insensitive)
        if (!string.IsNullOrWhiteSpace(clientName))
        {
            query = query.Where(f =>
                f.Client.Name.Contains(clientName)
            );
        }

        var totalCount = query.Count();

        var followUps = query
            .OrderBy(f => f.NextFollowUpDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new
        {
            page,
            pageSize,
            totalCount,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            data = followUps
        });
    }

    // POST: api/followups/
    [HttpPost]
    public IActionResult CreateFollowUp(CreateFollowUpDto dto)
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        // Ensure client belongs to logged-in user
        var client = _context.Clients
            .FirstOrDefault(c => c.Id == dto.ClientId && c.UserId == userId);

        if (client == null)
            return NotFound("Client not found");

        //filling up the followup model with the data received in createfollowupDTO from the frontend and then saving it in the database
        var followUp = new Followup
        {
            ClientId = dto.ClientId,
            Reason = dto.Reason,
            NextFollowUpDate = dto.NextFollowUpDate,
            Status = FollowUpStatus.Pending
        };

        _context.FollowUps.Add(followUp);
        _context.SaveChanges();

        return Ok(followUp);
    }

    // GET: api/followups/today
    [HttpGet("today")]
    public IActionResult GetTodayFollowUps()
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var today = DateTime.UtcNow.Date;

        //getting all the followups posted by user that has nextfollowupdate as today's date and status is pending
        var followUps = _context.FollowUps
            .Include(f => f.Client)
            .Where(f =>
                f.Client.UserId == userId &&
                f.Status == FollowUpStatus.Pending &&
                f.NextFollowUpDate.Date == today
            )
            .OrderBy(f => f.NextFollowUpDate)
            .ToList();

        return Ok(followUps);
    }

    // GET: api/followups/overdue
    [HttpGet("overdue")]
    public IActionResult GetOverdueFollowUps()
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var user = _context.Users
    .AsNoTracking()
    .FirstOrDefault(u => u.Id == userId);

        if (user == null)
            return Unauthorized();

        int overdueLimit = user.Plan == UserPlan.Free ? 3 : int.MaxValue;

        var today = DateTime.UtcNow.Date;

        var overdueFollowUps = _context.FollowUps
      .Include(f => f.Client)
      .Where(f =>
          f.Client.UserId == userId &&
          f.Status == FollowUpStatus.Pending &&
          f.NextFollowUpDate.Date < today
      )
      .OrderBy(f => f.NextFollowUpDate)
      .Take(overdueLimit)
      .Select(f => new OverdueFollowUpDto
      {
          FollowUpId = f.Id,
          ClientName = f.Client.Name,
          Reason = f.Reason,
          DueDate = f.NextFollowUpDate
      })
      .ToList();

        return Ok(overdueFollowUps);
    }

    // PUT: api/followups/{id}/complete
    [HttpPut("{id}/complete")]
    public IActionResult MarkComplete(Guid id)
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var followUp = _context.FollowUps
            .Include(f => f.Client)
            .FirstOrDefault(f =>
                f.Id == id &&
                f.Client.UserId == userId
            );

        if (followUp == null)
            return NotFound();

        followUp.Status = FollowUpStatus.Done;
        _context.SaveChanges();

        return NoContent();
    }

    [HttpPut("{id}")]
    public IActionResult EditFollowUp(Guid id, EditFollowUpDto dto)
    {
        // 1️⃣ Extract UserId from JWT
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        // 2️⃣ Fetch follow-up owned by this user
        var followUp = _context.FollowUps
            .Include(f => f.Client)
            .FirstOrDefault(f =>
                f.Id == id &&
                f.Client.UserId == userId
            );

        // 3️⃣ Ownership check
        if (followUp == null)
            return NotFound("Follow-up not found");

        // 4️⃣ Update allowed fields only
        followUp.Reason = dto.Reason;
        followUp.NextFollowUpDate = dto.NextFollowUpDate;
        followUp.Status = dto.Status;

        // 5️⃣ Save changes
        _context.SaveChanges();

        // 6️⃣ Return updated follow-up
        return Ok(followUp);
    }

    // DELETE: api/followups/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteFollowUp(Guid id)
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var followUp = _context.FollowUps
            .Include(f => f.Client)
            .FirstOrDefault(f =>
                f.Id == id &&
                f.Client.UserId == userId
            );

        if (followUp == null)
            return NotFound();

        _context.FollowUps.Remove(followUp);
        _context.SaveChanges();

        return NoContent();
    }
}