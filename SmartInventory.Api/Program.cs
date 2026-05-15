using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartInventory.Api.Endpoints;
using SmartInventory.Api.Hubs;
using SmartInventory.Api.Jobs;
using SmartInventory.Infrastructure.Data;
using SmartInventory.Infrastructure.Services;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Paste JWT token only. Do not include Bearer.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// SQL Server
builder.Services.AddDbContext<SmartInventoryDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SmartInventoryDb"));
});

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var connectionString = builder.Configuration["Redis:ConnectionString"];

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Redis connection string is missing.");
    }

    return ConnectionMultiplexer.Connect(connectionString);
});

// Hangfire
builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(
        builder.Configuration.GetConnectionString("SmartInventoryDb"));
});

builder.Services.AddHangfireServer();

// JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["Jwt:Key"]!;
    var issuer = builder.Configuration["Jwt:Issuer"]!;
    var audience = builder.Configuration["Jwt:Audience"]!;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddSignalR();

builder.Services.AddScoped<RedisLockService>();
builder.Services.AddScoped<ReservationExpirationJob>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRCors", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7176",
                "http://localhost:5173",
                "https://localhost:5173",
                "null"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Apply EF Core migrations before Hangfire recurring jobs use the database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SmartInventoryDbContext>();
    db.Database.Migrate();
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("SignalRCors");

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AllowAllHangfireAuthorizationFilter() }
});

app.MapHub<InventoryHub>("/hubs/inventory");

// Endpoints
app.MapGet("/", () => "Smart Inventory Reservation API is running");

app.MapProductEndpoints();
app.MapReservationEndpoints();
app.MapStockEndpoints();
app.MapAuthEndpoints();

RecurringJob.AddOrUpdate<ReservationExpirationJob>(
    "release-expired-reservations",
    job => job.ReleaseExpiredReservationsAsync(),
    Cron.Minutely);

app.Run();

public class AllowAllHangfireAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        return true;
    }
}