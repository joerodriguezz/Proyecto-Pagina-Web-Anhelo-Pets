using Microsoft.EntityFrameworkCore;
using AnheloPets.API.Data;
using AnheloPets.API.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Controllers
builder.Services.AddControllers();
builder.Services.AddScoped<IAnimalService, AnimalService>();
builder.Services.AddScoped<IRescateService, RescateService>();
builder.Services.AddScoped<IVolunteerService, VolunteerService>();
builder.Services.AddScoped<IAdoptionService, AdoptionService>();
builder.Services.AddScoped<IDonationService, DonationService>();
builder.Services.AddScoped<IUserService, UserService>();

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

if (connectionStringBuilder.Host.Contains("pooler.supabase.com", StringComparison.OrdinalIgnoreCase))
{
    connectionStringBuilder.Pooling = false;
}

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
