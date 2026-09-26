using API.Cors;
using API.Data;
using Microsoft.EntityFrameworkCore;
using API.Vault;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("CorsSettings.json", optional: false, reloadOnChange: true);

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

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAppCors(builder.Configuration);

// Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
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

app.UseAuthorization();

app.MapControllers();

app.Run();
