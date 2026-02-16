using Microsoft.EntityFrameworkCore;
using FollowUp.Api.Models;

namespace FollowUp.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
}
