using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskForge.Api.Data;
using TaskForge.Api.Domain;
using TaskForge.Api.Dtos;
using TaskForge.Api.Services;

namespace TaskForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    AppDbContext db,
    IPasswordHasher<User> passwordHasher,
    TokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(x => x.Email == email, cancellationToken))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Email already registered",
                Detail = "Use a different email address or sign in."
            });
        }

        var user = new User
        {
            Email = email,
            PasswordHash = string.Empty
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            tokenService.CreateToken(user));
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);

        if (user is null)
        {
            return Unauthorized(new ProblemDetails { Title = "Invalid credentials" });
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new ProblemDetails { Title = "Invalid credentials" });
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            await db.SaveChangesAsync(cancellationToken);
        }

        return Ok(tokenService.CreateToken(user));
    }
}
