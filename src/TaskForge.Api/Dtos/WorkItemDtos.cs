using System.ComponentModel.DataAnnotations;
using TaskForge.Api.Domain;

namespace TaskForge.Api.Dtos;

public sealed class CreateWorkItemRequest
{
    [Required, MinLength(2), MaxLength(160)]
    public string Title { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; init; }

    public WorkItemPriority Priority { get; init; } = WorkItemPriority.Medium;

    public DateTime? DueAtUtc { get; init; }
}

public sealed class UpdateWorkItemRequest
{
    [Required, MinLength(2), MaxLength(160)]
    public string Title { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; init; }

    public WorkItemStatus Status { get; init; }

    public WorkItemPriority Priority { get; init; }

    public DateTime? DueAtUtc { get; init; }
}

public sealed record WorkItemResponse(
    Guid Id,
    Guid ProjectId,
    string Title,
    string? Description,
    WorkItemStatus Status,
    WorkItemPriority Priority,
    DateTime? DueAtUtc,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record PagedResponse<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);
