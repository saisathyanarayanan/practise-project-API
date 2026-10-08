var builder = WebApplication.CreateBuilder(args);

// 1. Tell ASP.NET Core to use YARP and read the "ReverseProxy" section from appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// 2. Add CORS so your UI can communicate with the gateway freely
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

// 3. Map the Reverse Proxy into the request pipeline
app.MapReverseProxy();

app.Run();