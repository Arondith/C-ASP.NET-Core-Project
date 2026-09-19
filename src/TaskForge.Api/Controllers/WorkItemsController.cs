using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskForge.Api.Data;
using TaskForge.Api.Domain;
using TaskForge.Api.Dtos;
using TaskForge.Api.Services;

namespace TaskForge.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}/work-items")]
public sealed class WorkItemsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<WorkItemResponse>>> GetAll(
        Guid projectId,
        [FromQuery] WorkItemStatus? status,
        [FromQuery] WorkItemPriority? priority,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!await CanAccessProject(projectId, cancellationToken))
        {
            return NotFound();
        }

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.WorkItems
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId);

        if (status is not null)
        {
            query = query.Where(x => x.Status == status);
        }

        if (priority is not null)
        {
            query = query.Where(x => x.Priority == priority);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.Title.Contains(term) ||
                (x.Description != null && x.Description.Contains(term)));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.DueAtUtc)
            .ThenByDescending(x => x.UpdatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        return Ok(new PagedResponse<WorkItemResponse>(
            items,
            page,
            pageSize,
            totalItems,
            totalPages));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkItemResponse>> GetById(
        Guid projectId,
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!await CanAccessProject(projectId, cancellationToken))
        {
            return NotFound();
        }

        var item = await db.WorkItems
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == id && x.ProjectId == projectId,
                cancellationToken);

        return item is null ? NotFound() : Ok(ToResponse(item));
    }

    [HttpPost]
    public async Task<ActionResult<WorkItemResponse>> Create(
        Guid projectId,
        CreateWorkItemRequest request,
        CancellationToken cancellationToken)
    {
        if (!await CanAccessProject(projectId, cancellationToken))
        {
            return NotFound();
        }

        var item = new WorkItem
        {
            ProjectId = projectId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Priority = request.Priority,
            DueAtUtc = request.DueAtUtc
        };

        db.WorkItems.Add(item);
        await TouchProject(projectId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { projectId, id = item.Id },
            ToResponse(item));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WorkItemResponse>> Update(
        Guid projectId,
        Guid id,
        UpdateWorkItemRequest request,
        CancellationToken cancellationToken)
    {
        if (!await CanAccessProject(projectId, cancellationToken))
        {
            return NotFound();
        }

        var item = await db.WorkItems.SingleOrDefaultAsync(
            x => x.Id == id && x.ProjectId == projectId,
            cancellationToken);

        if (item is null)
        {
            return NotFound();
        }

        if (!WorkItemRules.CanTransition(item.Status, request.Status))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid status transition",
                Detail = $"Cannot move a work item from {item.Status} to {request.Status}."
            });
        }

        item.Title = request.Title.Trim();
        item.Description = request.Description?.Trim();
        item.Status = request.Status;
        item.Priority = request.Priority;
        item.DueAtUtc = request.DueAtUtc;
        item.UpdatedAtUtc = DateTime.UtcNow;

        await TouchProject(projectId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(item));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid projectId,
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!await CanAccessProject(projectId, cancellationToken))
        {
            return NotFound();
        }

        var item = await db.WorkItems.SingleOrDefaultAsync(
            x => x.Id == id && x.ProjectId == projectId,
            cancellationToken);

        if (item is null)
        {
            return NotFound();
        }

        db.WorkItems.Remove(item);
        await TouchProject(projectId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task<bool> CanAccessProject(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(rawUserId, out var userId))
        {
            return false;
        }

        var isAdmin = User.IsInRole("Admin");

        return await db.Projects.AnyAsync(
            x => x.Id == projectId && (isAdmin || x.OwnerId == userId),
            cancellationToken);
    }

    private async Task TouchProject(Guid projectId, CancellationToken cancellationToken)
    {
        var project = await db.Projects.SingleAsync(x => x.Id == projectId, cancellationToken);
        project.UpdatedAtUtc = DateTime.UtcNow;
    }

    private static WorkItemResponse ToResponse(WorkItem item) =>
        new(
            item.Id,
            item.ProjectId,
            item.Title,
            item.Description,
            item.Status,
            item.Priority,
            item.DueAtUtc,
            item.CreatedAtUtc,
            item.UpdatedAtUtc);
}
