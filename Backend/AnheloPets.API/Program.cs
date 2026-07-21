using Microsoft.EntityFrameworkCore;
using AnheloPets.API.Data;
using AnheloPets.API.Services;
using AnheloPets.API.Repository;
using AnheloPets.API.Middleware;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Controllers
builder.Services.AddControllers();
builder.Services.AddScoped<AuthRepository>();
builder.Services.AddScoped<AnimalRepository>();
builder.Services.AddScoped<RescueRepository>();
builder.Services.AddScoped<FosterHomeRepository>();
builder.Services.AddScoped<AnimalMedicalRecordRepository>();
builder.Services.AddScoped<VeterinarianRepository>();
builder.Services.AddScoped<RoleRepository>();
builder.Services.AddScoped<UserAdminRepository>();
builder.Services.AddScoped<VolunteerRepository>();
builder.Services.AddScoped<IAnimalMedicalRecordService, AnimalMedicalRecordService>();
builder.Services.AddScoped<IVeterinarianService, VeterinarianService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserAdminService, UserAdminService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAnimalService, AnimalService>();
builder.Services.AddScoped<IRescateService, RescateService>();
builder.Services.AddScoped<IVolunteerService, VolunteerService>();
builder.Services.AddScoped<IFosterHomeService, FosterHomeService>();
builder.Services.AddScoped<IFosterPlacementService, FosterPlacementService>();
builder.Services.AddScoped<IAdoptionService, AdoptionService>();
builder.Services.AddScoped<IDonationService, DonationService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Base de datos PostgreSQL
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(defaultConnection))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is not configured. " +
        "Set ConnectionStrings__DefaultConnection in the deployment environment.");
}

var connectionStringBuilder = new NpgsqlConnectionStringBuilder(defaultConnection);
if (string.IsNullOrWhiteSpace(connectionStringBuilder.SearchPath))
{
    connectionStringBuilder.SearchPath = "anhelopets, public";
}

connectionStringBuilder.Timeout = Math.Max(connectionStringBuilder.Timeout, 15);
connectionStringBuilder.CommandTimeout = Math.Max(connectionStringBuilder.CommandTimeout, 60);


builder.Services.AddDbContext<AnheloPetsDbContext>(options =>
    options.UseNpgsql(connectionStringBuilder.ConnectionString));

// CORS para Vue
builder.Services.AddCors(options =>
{
    options.AddPolicy("VuePolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ── Bootstrap temporal: aplica a la BD ya provisionada el esquema de
// voluntariado corregido (ver database/tables.sql). Idempotente. Se retira
// tras confirmarse.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AnheloPetsDbContext>();
    db.Database.ExecuteSqlRaw("""
        ALTER TABLE anhelopets.volunteers ADD COLUMN IF NOT EXISTS application_details text;
        ALTER TABLE anhelopets.user_contacts ADD COLUMN IF NOT EXISTS district varchar(100);

        DO $$
        BEGIN
            IF EXISTS (
                SELECT 1 FROM information_schema.columns
                WHERE table_schema = 'anhelopets' AND table_name = 'volunteers'
                  AND column_name = 'validated_by_user_id' AND data_type <> 'text'
            ) THEN
                ALTER TABLE anhelopets.volunteers ALTER COLUMN validated_by_user_id TYPE text USING validated_by_user_id::text;
            END IF;
        END $$;
        """);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();



// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("VuePolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
