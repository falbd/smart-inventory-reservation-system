using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartInventory.Contracts.Auth;
using SmartInventory.Infrastructure.Data;
using SmartInventory.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;

namespace SmartInventory.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest request,
            SmartInventoryDbContext db,
            IConfiguration configuration) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.FullName))
            {
                return Results.BadRequest("Full name, email, and password are required.");
            }

            var role = string.IsNullOrWhiteSpace(request.Role)
                ? "Staff"
                : request.Role;

            if (role is not "Admin" and not "Staff")
                return Results.BadRequest("Role must be Admin or Staff.");

            var normalizedEmail = request.Email.Trim().ToLower();

            var emailExists = await db.AppUsers
                .AnyAsync(x => x.Email == normalizedEmail);

            if (emailExists)
                return Results.Conflict("Email already exists.");

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                Email = normalizedEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = role
            };

            db.AppUsers.Add(user);
            await db.SaveChangesAsync();

            var token = CreateJwtToken(user, configuration);

            return Results.Ok(new AuthResponse(
                user.Id,
                user.FullName,
                user.Email,
                user.Role,
                token
            ));
        });

        group.MapPost("/login", async (LoginRequest request,
            SmartInventoryDbContext db,
            IConfiguration configuration) =>
        {
            var user = await db.AppUsers
                .FirstOrDefaultAsync(x => x.Email == request.Email.Trim().ToLower());

            if (user is null)
                return Results.Unauthorized();

            var validPassword = BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash);

            if (!validPassword)
                return Results.Unauthorized();

            var token = CreateJwtToken(user, configuration);

            return Results.Ok(new AuthResponse(
                user.Id,
                user.FullName,
                user.Email,
                user.Role,
                token
            ));
        });

        // User Profile 
        group.MapGet("/me", async (ClaimsPrincipal userPrincipal,
          SmartInventoryDbContext db) =>
        {
            var userIdValue = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdValue, out var userId))
                return Results.Unauthorized();

            var user = await db.AppUsers
                .Where(x => x.Id == userId)
                .Select(x => new
                {
                    x.Id,
                    x.FullName,
                    x.Email,
                    x.Role,
                    x.CreatedAtUtc
                })
                .FirstOrDefaultAsync();

            return user is null
                ? Results.NotFound("User not found.")
                : Results.Ok(user);
         })
            .RequireAuthorization();


        group.MapPost("/logout", () =>
        {
            return Results.Ok(new
            {
                Message = "Logged out successfully. Remove the token from the client."
            });
        })
            .RequireAuthorization();

        return app;
    }

    private static string CreateJwtToken(AppUser user, IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"]!;
        var issuer = configuration["Jwt:Issuer"]!;
        var audience = configuration["Jwt:Audience"]!;
        var expiresInMinutes = int.Parse(configuration["Jwt:ExpiresInMinutes"]!);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
