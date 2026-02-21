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
    public IActionResult GetFollowUps()
    {
        // extracting the userid from JWT and converting it into GUID type
        // this gives the userid of the user who sent the api request
        // always extract the userid from JWT and never take it from Http request body as maicious users might intentially write a different userid in request body to get another user's data but they can never tamper with the userid stored in JWT
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        //getting all the followups posted by the user
        var followUps = _context.FollowUps
            .Include(f => f.Client)
            .Where(f => f.Client.UserId == userId)
            .OrderBy(f => f.NextFollowUpDate)
            .ToList();

        return Ok(followUps);  
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

    // POST: api/followups
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
}