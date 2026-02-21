using Microsoft.EntityFrameworkCore; // EF Core engine, gives access to DbContext, DbSet, ModelBuilder and lINQ async methods (AnyAsync, FirstOrDefaultAsync)
using FollowUp.Api.Models; 


namespace FollowUp.Api.Data;

public class AppDbContext : DbContext //represents a connection to the database
{

    // Receives configurations from program.cs like: which db, which provider, Connection string, migrations assemble
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    //represents Users Table 
    // enables: _context.Users.Add(), _context.Users.AnyAsync(), _context.Users.FirstOrDefaultAsync()
    public DbSet<User> Users => Set<User>();

    //represents Client Table 
    // enables: _context.Client.Add(), _context.Client.AnyAsync(), _context.Client.FirstOrDefaultAsync()
    public DbSet<Client> Clients => Set<Client>();


    //represents Followup Table 
    // enables: _context.Followup.Add(), _context.Followup.AnyAsync(), _context.Followup.FirstOrDefaultAsync()
    public DbSet<Followup> FollowUps => Set<Followup>();

    

    /*
     This runs when EF builds the model
    Used to define:
    indexes
    constraint
    relationships
    table names
    composite keys
     */
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);  //ensures default EF behaviour still runs


        //creates unique index on user(email) to prevent duplicate user in DB
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        //creates index on two columns
        modelBuilder.Entity<Followup>()
            .HasIndex(f => new { f.NextFollowUpDate, f.Status });
    }


}

