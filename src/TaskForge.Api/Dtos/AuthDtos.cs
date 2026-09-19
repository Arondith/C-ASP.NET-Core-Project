using System.ComponentModel.DataAnnotations;

namespace TaskForge.Api.Dtos;

public sealed class RegisterRequest
{
    [Required, EmailAddress, MaxLength(320)]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(10), MaxLength(100)]
    public string Password { get; init; } = string.Empty;
}

public sealed class LoginRequest
{
    [Required, EmailAddress, MaxLength(320)]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

public sealed record AuthResponse(
    Guid UserId,
    string Email,
    string Role,
    string Token,
    DateTime ExpiresAtUtc);
