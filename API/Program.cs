using API.Cors;
using API.Data;
using Microsoft.EntityFrameworkCore;
using API.Vault;
using API.Jwt;
using API.Repositories;
using API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("CorsSettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("JwtSettings.json", optional: false, reloadOnChange: true);

// 2. Load secrets from Vault into Configuration
builder.Configuration.AddVaultAppConfiguration();
// 3. Test printing secrets fetched from Vault:
Console.WriteLine("========================================");
Console.WriteLine("[Vault] Loaded JWT Secret: " + builder.Configuration["JwtConfig:secret"]);
Console.WriteLine("[Vault] Loaded Client ID:  " + builder.Configuration["JwtConfig:client_id"]);
Console.WriteLine("[Vault] Loaded DB Conn:    " + builder.Configuration.GetConnectionString("DefaultConnection"));
Console.WriteLine("========================================");  

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Application Layer Registrations - Repositories
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IDatabaseHealthRepository, DatabaseHealthRepository>();
builder.Services.AddScoped<IUserManagementRepository, UserManagementRepository>();
builder.Services.AddScoped<IPersonRepository, PersonRepository>();

// Application Layer Registrations - Services
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDatabaseHealthService, DatabaseHealthService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IArgon2PasswordService, Argon2PasswordService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IPersonService, PersonService>();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAppCors(builder.Configuration);
builder.Services.AddAppJwt(builder.Configuration);

// Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var personRepo = scope.ServiceProvider.GetRequiredService<IPersonRepository>();
    await personRepo.EnsureTablesExistAsync();
    Console.WriteLine("[DB] Verified Persons & IdempotencyRecords tables.");
}
catch (Exception ex)
{
    Console.WriteLine($"[DB Warning] Failed to ensure tables exist: {ex.Message}");
}

app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAppCors(app.Configuration);
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
