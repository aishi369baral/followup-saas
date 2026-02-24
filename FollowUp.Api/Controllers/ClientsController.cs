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
    [HttpGet]
    public IActionResult GetClients(
    int pageNumber = 1,
    int pageSize = 10
)
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var totalCount = _context.Clients
            .Count(c => c.UserId == userId);

        var clients = _context.Clients
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ClientResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Company = c.Company,
                Email = c.Email,
                Phone = c.Phone,
                Notes = c.Notes,
                CreatedAt = c.CreatedAt
            })
            .ToList();

        return Ok(new
        {
            totalCount,
            pageNumber,
            pageSize,
            clients
        });
    }

    // POST: api/clients
    [HttpPost]
    public IActionResult CreateClient(CreateClientDto dto)
    {
        var userId = Guid.Parse(
       User.FindFirstValue(ClaimTypes.NameIdentifier)!
   );

        var user = _context.Users
    .AsNoTracking()
    .FirstOrDefault(u => u.Id == userId);

        if (user == null)
            return Unauthorized();

        //getting the number of clients of the user
        var clientCount = _context.Clients.Count(c => c.UserId == userId);
       
        if (user.Plan == UserPlan.Free && clientCount >= 5)
            return BadRequest("Free tier limit reached (max 5 clients)");

        var client = new Client
        {
            UserId = userId,
            Name = dto.Name,
            Company = dto.Company,
            Email = dto.Email,
            Phone = dto.Phone,
            Notes = dto.Notes,
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

    [HttpPut("{id}")]
    public IActionResult UpdateClient(Guid id, UpdateClientDto dto)
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var client = _context.Clients
            .FirstOrDefault(c => c.Id == id && c.UserId == userId);

        if (client == null)
            return NotFound("Client not found");

        client.Name = dto.Name;
        client.Company = dto.Company;
        client.Email = dto.Email;
        client.Phone = dto.Phone;
        client.Notes = dto.Notes;
       
        

        _context.SaveChanges();
        return Ok(client);
    }


}