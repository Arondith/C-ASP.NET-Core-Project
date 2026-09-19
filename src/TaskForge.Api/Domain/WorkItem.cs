namespace TaskForge.Api.Domain;

public sealed class WorkItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public WorkItemStatus Status { get; set; } = WorkItemStatus.Todo;
    public WorkItemPriority Priority { get; set; } = WorkItemPriority.Medium;
    public DateTime? DueAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
