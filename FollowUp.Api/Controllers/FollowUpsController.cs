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
    // PURPOSE:
    // - Returns follow-ups for Follow-ups page
    // - Supports optional filters:
    //   • status
    //   • clientName
    //   • today
    // - SAME endpoint is reused for:
    //   • "View all today" from dashboard
    //   • Normal follow-up listing

    // /api/followups? status = Pending                             Followup page
    // / api / followups ? clientName = John                        Followup page
    // / api / followups ? today = true                             Dashboard today view all 
    // / api / followups ? status = Pending & clientName = John     Followup page
    [HttpGet]
    public IActionResult GetFollowUps(
        [FromQuery] FollowUpStatus? status,
        [FromQuery] string? clientName,
        [FromQuery] bool? today
    )
    {
        // Extract userId securely from JWT
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        // Base query: all follow-ups belonging to this user
        var query = _context.FollowUps
            .Include(f => f.Client)
            .Where(f => f.Client.UserId == userId);

        // Optional filter: Status
        if (status.HasValue)
        {
            query = query.Where(f => f.Status == status.Value);
        }

        // Optional filter: Client name (search)
        if (!string.IsNullOrWhiteSpace(clientName))
        {
            query = query.Where(f =>
                f.Client.Name.Contains(clientName)
            );
        }

        // Optional filter: Today only
        if (today == true)
        {
            var todayDate = DateTime.UtcNow.Date;
            query = query.Where(f =>
                f.NextFollowUpDate.Date == todayDate
            );
        }

        // Final ordering
        var followUps = query
            .OrderBy(f => f.NextFollowUpDate)
            .ToList();

        return Ok(followUps);
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