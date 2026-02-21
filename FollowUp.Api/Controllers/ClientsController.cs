using FollowUp.Api.Data;
using FollowUp.Api.DTO;
using FollowUp.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]                  //only authenticated users are authorized to call any of the below api endpoints. //we dont have to wrte auth checks manually middlware handles it.
public class ClientsController : ControllerBase
{
    private readonly AppDbContext _context;
   

    public ClientsController(AppDbContext context)
    {
        _context = context;
        
    }

    // GET: api/clients
    [HttpGet]
    public IActionResult GetClients()
    {
        // extracting the userid from JWT and converting it into GUID type
        // this gives the userid of the user who sent the api request
        // always extract the userid from JWT and never take it from Http request body as maicious users might intentially write a different userid in request body to get another user's data but they can never tamper with the userid stored in JWT
        var userId = Guid.Parse(
     User.FindFirstValue(ClaimTypes.NameIdentifier)!
 );

        var clients = _context.Clients
            .Where(c => c.UserId == userId)
            .ToList();

        return Ok(clients);
    }

    // POST: api/clients
    [HttpPost]
    public IActionResult CreateClient(CreateClientDto dto)
    {
        var userId = Guid.Parse(
       User.FindFirstValue(ClaimTypes.NameIdentifier)!
   );

        //getting the number of clients of the user
        var clientCount = _context.Clients.Count(c => c.UserId == userId);

        if (clientCount >= 5)
            return BadRequest("Free tier limit reached (max 5 clients)");

        var client = new Client
        {
            Name = dto.Name,
            UserId = userId
        };

        _context.Clients.Add(client);
        _context.SaveChanges();

        return Ok(client);
    }

    // DELETE: api/clients/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteClient(Guid id)
    {
        var userId = Guid.Parse(
     User.FindFirstValue(ClaimTypes.NameIdentifier)!
 );

        var client = _context.Clients
            .FirstOrDefault(c => c.Id == id && c.UserId == userId);

        if (client == null)
            return NotFound();

        _context.Clients.Remove(client);
        _context.SaveChanges();

        return NoContent();
    }
}