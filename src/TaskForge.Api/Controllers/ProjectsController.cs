using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskForge.Api.Data;
using TaskForge.Api.Domain;
using TaskForge.Api.Dtos;

namespace TaskForge.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class ProjectsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProjectResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var isAdmin = User.IsInRole("Admin");

        var projects = await db.Projects
            .AsNoTracking()
            .Where(x => isAdmin || x.OwnerId == userId)
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Select(x => new ProjectResponse(
                x.Id,
                x.Name,
                x.Description,
                x.WorkItems.Count,
                x.CreatedAtUtc,
                x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(projects);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var isAdmin = User.IsInRole("Admin");

        var project = await db.Projects
            .AsNoTracking()
            .Where(x => x.Id == id && (isAdmin || x.OwnerId == userId))
            .Select(x => new ProjectResponse(
                x.Id,
                x.Name,
                x.Description,
                x.WorkItems.Count,
                x.CreatedAtUtc,
                x.UpdatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);

        return project is null ? NotFound() : Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> Create(
        CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var project = new Project
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            OwnerId = userId.Value
        };

        db.Projects.Add(project);
        await db.SaveChangesAsync(cancellationToken);

        var response = ToResponse(project, 0);
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProjectResponse>> Update(
        Guid id,
        UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var project = await FindOwnedProject(id, cancellationToken);
        if (project is null) return NotFound();

        project.Name = request.Name.Trim();
        project.Description = request.Description?.Trim();
        project.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        var workItemCount = await db.WorkItems.CountAsync(
            x => x.ProjectId == project.Id,
            cancellationToken);

        return Ok(ToResponse(project, workItemCount));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var project = await FindOwnedProject(id, cancellationToken);
        if (project is null) return NotFound();

        db.Projects.Remove(project);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpGet("{id:guid}/stats")]
    public async Task<ActionResult<ProjectStatsResponse>> GetStats(
        Guid id,
        CancellationToken cancellationToken)
    {
        var project = await FindOwnedProject(id, cancellationToken, track: false);
        if (project is null) return NotFound();

        var workItems = db.WorkItems.AsNoTracking().Where(x => x.ProjectId == id);

        var response = new ProjectStatsResponse(
            id,
            await workItems.CountAsync(cancellationToken),
            await workItems.CountAsync(x => x.Status == WorkItemStatus.Todo, cancellationToken),
            await workItems.CountAsync(x => x.Status == WorkItemStatus.InProgress, cancellationToken),
            await workItems.CountAsync(x => x.Status == WorkItemStatus.Blocked, cancellationToken),
            await workItems.CountAsync(x => x.Status == WorkItemStatus.Done, cancellationToken),
            await workItems.CountAsync(x => x.Status == WorkItemStatus.Cancelled, cancellationToken));

        return Ok(response);
    }

    private async Task<Project?> FindOwnedProject(
        Guid id,
        CancellationToken cancellationToken,
        bool track = true)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return null;

        var isAdmin = User.IsInRole("Admin");
        var query = track ? db.Projects.AsQueryable() : db.Projects.AsNoTracking();

        return await query.SingleOrDefaultAsync(
            x => x.Id == id && (isAdmin || x.OwnerId == userId),
            cancellationToken);
    }

    private Guid? GetCurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out var id) ? id : null;
    }

    private static ProjectResponse ToResponse(Project project, int count) =>
        new(
            project.Id,
            project.Name,
            project.Description,
            count,
            project.CreatedAtUtc,
            project.UpdatedAtUtc);
}
