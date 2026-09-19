using System.ComponentModel.DataAnnotations;

namespace TaskForge.Api.Dtos;

public sealed class CreateProjectRequest
{
    [Required, MinLength(2), MaxLength(120)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; init; }
}

public sealed class UpdateProjectRequest
{
    [Required, MinLength(2), MaxLength(120)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; init; }
}

public sealed record ProjectResponse(
    Guid Id,
    string Name,
    string? Description,
    int WorkItemCount,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record ProjectStatsResponse(
    Guid ProjectId,
    int Total,
    int Todo,
    int InProgress,
    int Blocked,
    int Done,
    int Cancelled);
