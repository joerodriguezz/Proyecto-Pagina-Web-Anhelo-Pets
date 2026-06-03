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

    public DbSet<Animal> Animals { get; set; }

    public DbSet<Volunteer> Volunteers { get; set; }
}
