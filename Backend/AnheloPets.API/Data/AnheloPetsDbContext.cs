using Microsoft.EntityFrameworkCore;
using AnheloPets.API.Models;

namespace AnheloPets.API.Data;

public class AnheloPetsDbContext : DbContext
{
    public AnheloPetsDbContext(
        DbContextOptions<AnheloPetsDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    
    public DbSet<UserProfile> UserProfiles { get; set; }
    
    public DbSet<UserContacts > UserContacts { get; set; }

    public DbSet<Animal> Animals { get; set; }

    public DbSet<Volunteer> Volunteers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .Property(u => u.UserId)
            .HasDefaultValueSql("generate_user_id()")
            .ValueGeneratedOnAdd();
    }
}
