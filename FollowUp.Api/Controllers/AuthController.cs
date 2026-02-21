using FollowUp.Api.Auth;
using FollowUp.Api.Data;
using FollowUp.Api.Dtos;
using FollowUp.Api.Models;
using Microsoft.AspNetCore.Mvc; // required for ControllerBase, IActionResult, routing attributes
using Microsoft.EntityFrameworkCore; //enables async EF Core methods like: AnyAsync, FirstOrDefaultAync

namespace FollowUp.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwt;

    public AuthController(AppDbContext context, JwtService jwt)            // dependency injection happening
    {
        _context = context;
        _jwt = jwt;
    }




    /*
     Register flow:
    client -> /register
    ---> user saved in DB
    ---> password safely hashed
     */
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("Email already exists");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok();
    }



    /*
     Login flow:
    Client -> /login
    ----> credentials verified
    ----> JWT issued
    ----> user is now authenticated for future requests
     */
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null ||
            !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");

        var token = _jwt.GenerateToken(user);

        return Ok(new { token });
    }
}
