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
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Followup> FollowUps => Set<Followup>();

    // 3️⃣ Indexes & constraints (EF configuration)
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Followup>()
            .HasIndex(f => new { f.NextFollowUpDate, f.Status });
    }


}

