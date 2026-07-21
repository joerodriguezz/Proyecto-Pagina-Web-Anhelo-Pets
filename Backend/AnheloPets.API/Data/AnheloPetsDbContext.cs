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

    public DbSet<Veterinarian> Veterinarians { get; set; }

    public DbSet<RescueRecord> RescueRecords { get; set; }

    public DbSet<FosterHome> FosterHomes { get; set; }

    public DbSet<AnimalMedicalRecord> AnimalMedicalRecords { get; set; }

    public DbSet<Role> Roles { get; set; }

    public DbSet<UserRole> UserRoles { get; set; }

    public DbSet<Donation> Donations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("anhelopets");

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.Property(u => u.UserId)
                .HasColumnName("user_id")
                .HasDefaultValueSql("generate_user_id()")
                .ValueGeneratedOnAdd();
            entity.Property(u => u.Active).HasColumnName("active");
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("user_profiles");
        });

        modelBuilder.Entity<UserContacts>(entity =>
        {
            entity.ToTable("user_contacts");
        });

        modelBuilder.Entity<Animal>(entity =>
        {
            entity.ToTable("animals");
            entity.Property(a => a.AnimalId)
                .HasColumnName("animal_id")
                .HasDefaultValueSql("generate_id('ANM')")
                .ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Volunteer>(entity =>
        {
            entity.ToTable("volunteers");
            entity.Property(v => v.VolunteerId)
                .HasColumnName("volunteer_id")
                .HasDefaultValueSql("generate_id('VOL')")
                .ValueGeneratedOnAdd();
            entity.Property(v => v.UserId).HasColumnName("user_id").HasColumnType("text");
            entity.Property(v => v.Active).HasColumnName("active");
            entity.Property(v => v.NationalId).HasColumnName("national_id").HasMaxLength(50);
            entity.Property(v => v.VolunteerType).HasColumnName("volunteer_type").HasMaxLength(100);
            entity.Property(v => v.Motivation).HasColumnName("motivation");
            entity.Property(v => v.ApplicationDetails).HasColumnName("application_details");
            entity.Property(v => v.ValidationStatus).HasColumnName("validation_status").HasMaxLength(20);
            entity.Property(v => v.ValidationNotes).HasColumnName("validation_notes");
            entity.Property(v => v.ValidatedAt).HasColumnName("validated_at");
            entity.Property(v => v.ValidatedByUserId).HasColumnName("validated_by_user_id").HasColumnType("text");
            entity.Property(v => v.CreatedAt).HasColumnName("created_at");
            entity.Property(v => v.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(v => v.ModifiedAt).HasColumnName("modified_at");
            entity.Property(v => v.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        });

        modelBuilder.Entity<Veterinarian>(entity =>
        {
            entity.ToTable("veterinarians");
            entity.Property(v => v.VeterinarianId)
                .HasColumnName("veterinarian_id")
                .HasDefaultValueSql("generate_id('VET')")
                .ValueGeneratedOnAdd();
            entity.Property(v => v.VolunteerId).HasColumnName("volunteer_id").HasColumnType("text");
            entity.Property(v => v.Specialty).HasColumnName("specialty").HasMaxLength(100);
            entity.Property(v => v.CreatedAt).HasColumnName("created_at");
            entity.Property(v => v.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(v => v.ModifiedAt).HasColumnName("modified_at");
            entity.Property(v => v.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);

            entity.HasIndex(v => v.VolunteerId).IsUnique();
        });

        modelBuilder.Entity<FosterHome>(entity =>
        {
            entity.ToTable("foster_homes");
            entity.Property(f => f.FosterHomeId)
                .HasColumnName("foster_home_id")
                .HasDefaultValueSql("generate_id('FHM')")
                .ValueGeneratedOnAdd();
            entity.Property(f => f.VolunteerId).HasColumnName("volunteer_id").HasColumnType("text");
            entity.Property(f => f.Name).HasColumnName("name").HasColumnType("varchar(150)");
            entity.Property(f => f.Address).HasColumnName("address").HasColumnType("text");
            entity.Property(f => f.Phone).HasColumnName("phone").HasColumnType("varchar(30)");
            entity.Property(f => f.Responsible).HasColumnName("responsible").HasColumnType("varchar(150)");
            entity.Property(f => f.Capacity).HasColumnName("capacity").HasColumnType("integer");
            entity.Property(f => f.Active).HasColumnName("active").HasColumnType("boolean");
            entity.Property(f => f.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz");
            entity.Property(f => f.CreatedBy).HasColumnName("created_by").HasColumnType("varchar(100)");
            entity.Property(f => f.ModifiedAt).HasColumnName("modified_at").HasColumnType("timestamptz");
            entity.Property(f => f.ModifiedBy).HasColumnName("modified_by").HasColumnType("varchar(100)");
        });

        modelBuilder.Entity<AnimalMedicalRecord>(entity =>
        {
            entity.ToTable("animal_medical_records");
            entity.Property(r => r.AnimalMedicalRecordId)
                .HasColumnName("animal_medical_record_id")
                .ValueGeneratedOnAdd();
            entity.Property(r => r.AnimalId).HasColumnName("animal_id");
            entity.Property(r => r.VeterinarianId).HasColumnName("veterinarian_id");
            entity.Property(r => r.Diagnosis).HasColumnName("diagnosis");
            entity.Property(r => r.Treatment).HasColumnName("treatment");
            entity.Property(r => r.Notes).HasColumnName("notes");
            entity.Property(r => r.VisitDate).HasColumnName("visit_date");
            entity.Property(r => r.CreatedAt).HasColumnName("created_at");
            entity.Property(r => r.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(r => r.ModifiedAt).HasColumnName("modified_at");
            entity.Property(r => r.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.Property(r => r.RoleId).HasColumnName("role_id").ValueGeneratedOnAdd();
            entity.Property(r => r.RoleName).HasColumnName("role_name").HasMaxLength(100);
            entity.Property(r => r.RoleAccess).HasColumnName("role_access").HasMaxLength(100);
            entity.Property(r => r.Description).HasColumnName("description");
            entity.Property(r => r.CreatedAt).HasColumnName("created_at");
            entity.Property(r => r.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(r => r.ModifiedAt).HasColumnName("modified_at");
            entity.Property(r => r.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);

            entity.HasIndex(r => r.RoleName).IsUnique();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles");
            entity.Property(ur => ur.UserRoleId).HasColumnName("user_role_id").ValueGeneratedOnAdd();
            entity.Property(ur => ur.UserId).HasColumnName("user_id").HasColumnType("text");
            entity.Property(ur => ur.RoleId).HasColumnName("role_id");
            entity.Property(ur => ur.Description).HasColumnName("description");
            entity.Property(ur => ur.CreatedAt).HasColumnName("created_at");
            entity.Property(ur => ur.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(ur => ur.ModifiedAt).HasColumnName("modified_at");
            entity.Property(ur => ur.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);

            entity.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
        });

        modelBuilder.Entity<RescueRecord>(entity =>
        {
            entity.ToTable("rescue_records");
            entity.Property(r => r.RescueId)
                .HasColumnName("rescue_id")
                .ValueGeneratedOnAdd();
            entity.Property(r => r.AnimalId).HasColumnName("animal_id");
            entity.Property(r => r.RescueDate).HasColumnName("rescue_date");
            entity.Property(r => r.Location).HasColumnName("location");
            entity.Property(r => r.Description).HasColumnName("description");
            entity.Property(r => r.Status).HasColumnName("status").HasMaxLength(30);
            entity.Property(r => r.FosterHomeId).HasColumnName("foster_home_id").HasColumnType("text");
            entity.Property(r => r.VolunteerId).HasColumnName("volunteer_id").HasColumnType("text");
            entity.Property(r => r.CreatedAt).HasColumnName("created_at");
            entity.Property(r => r.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(r => r.ModifiedAt).HasColumnName("modified_at");
            entity.Property(r => r.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        });

        modelBuilder.Entity<Donation>(entity =>
        {
            entity.ToTable("donations");
            entity.Property(d => d.DonationId).HasColumnName("donation_id").ValueGeneratedOnAdd();
            entity.Property(d => d.Currency).HasColumnName("currency").HasMaxLength(3);
            entity.Property(d => d.Method).HasColumnName("method").HasMaxLength(50);
            entity.Property(d => d.ValidationStatus).HasColumnName("validation_status").HasMaxLength(20);
            entity.Property(d => d.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(d => d.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        });
    }
}
