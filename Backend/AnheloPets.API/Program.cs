using Microsoft.EntityFrameworkCore;
using AnheloPets.API.Data;
using AnheloPets.API.Services;
using AnheloPets.API.Repository;
using AnheloPets.API.Middleware;
using Npgsql;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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
builder.Services.AddScoped<DonationRepository>();
builder.Services.AddScoped<AdoptionRequestRepository>();
builder.Services.AddScoped<AnimalFosterPlacementRepository>();
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
builder.Services.AddScoped<IDonationService, DonationService>();
builder.Services.AddScoped<IAdoptionRequestService, AdoptionRequestService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAnimalPhotoService, AnimalPhotoService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Authentication
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"];
if (string.IsNullOrWhiteSpace(jwtSigningKey))
{
    throw new InvalidOperationException(
        "Jwt:SigningKey is not configured. Set Jwt__SigningKey in the deployment environment.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// Supabase Storage
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseServiceKey = builder.Configuration["Supabase:ServiceKey"];
if (string.IsNullOrWhiteSpace(supabaseUrl) || string.IsNullOrWhiteSpace(supabaseServiceKey))
{
    throw new InvalidOperationException(
        "Supabase:Url / Supabase:ServiceKey no configurados. Usa `dotnet user-secrets set Supabase:ServiceKey ...` " +
        "en desarrollo, o Supabase__Url / Supabase__ServiceKey en el entorno de despliegue.");
}

builder.Services.AddHttpClient("Supabase", client =>
{
    client.BaseAddress = new Uri(supabaseUrl);
    client.DefaultRequestHeaders.Add("apikey", supabaseServiceKey);
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseServiceKey}");
});
builder.Services.AddScoped<ISupabaseStorageService, SupabaseStorageService>();

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
        policy.WithOrigins(
                  "https://proyecto-pagina-web-anhelo-pets.vercel.app",
                  "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();



// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("VuePolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
