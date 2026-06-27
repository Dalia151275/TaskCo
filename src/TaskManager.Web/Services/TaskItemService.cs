using Microsoft.EntityFrameworkCore;
using TaskManager.Web.Common.Results;
using TaskManager.Web.Data;
using TaskManager.Web.Models.Dtos.TaskItems;
using TaskManager.Web.Models.Entities;
using TaskManager.Web.Services.Abstractions;

namespace TaskManager.Web.Services;

public sealed class TaskItemService : ITaskItemService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public TaskItemService(AppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<TaskItemResponse>>> ListAsync(Guid projectId)
    {
        var projectExists = await _db.Projects
            .AnyAsync(p => p.Id == projectId && p.OwnerId == _currentUser.UserId);

        if (!projectExists)
            return new Error(ErrorCodes.NotFound, "Project not found.");

        var tasks = await _db.TaskItems
            .Where(t => t.ProjectId == projectId && t.OwnerId == _currentUser.UserId)
            .OrderBy(t => t.CreatedAt)
            .Select(t => new TaskItemResponse(
                t.Id, t.Title, t.Description, t.Status, t.DueDate,
                t.ProjectId, t.CreatedAt, t.UpdatedAt))
            .ToListAsync();

        return tasks.AsReadOnly();
    }

    public async Task<Result<TaskItemResponse>> GetByIdAsync(Guid projectId, Guid id)
    {
        var task = await FindOwnedAsync(projectId, id);
        if (task is null)
            return new Error(ErrorCodes.NotFound, "Task not found.");

        return ToResponse(task);
    }

    public async Task<Result<TaskItemResponse>> CreateAsync(Guid projectId, CreateTaskItemRequest request)
    {
        // Load the parent project filtered by owner; copies OwnerId — never read from client.
        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.OwnerId == _currentUser.UserId);

        if (project is null)
            return new Error(ErrorCodes.NotFound, "Project not found.");

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            DueDate = request.DueDate,
            ProjectId = projectId,
            OwnerId = project.OwnerId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.TaskItems.Add(task);
        await _db.SaveChangesAsync();

        return ToResponse(task);
    }

    public async Task<Result<TaskItemResponse>> UpdateAsync(Guid projectId, Guid id, UpdateTaskItemRequest request)
    {
        var task = await FindOwnedAsync(projectId, id);
        if (task is null)
            return new Error(ErrorCodes.NotFound, "Task not found.");

        task.Title = request.Title;
        task.Description = request.Description;
        task.Status = request.Status;
        task.DueDate = request.DueDate;
        task.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        return ToResponse(task);
    }

    public async Task<Result<bool>> DeleteAsync(Guid projectId, Guid id)
    {
        var task = await FindOwnedAsync(projectId, id);
        if (task is null)
            return new Error(ErrorCodes.NotFound, "Task not found.");

        _db.TaskItems.Remove(task);
        await _db.SaveChangesAsync();

        return true;
    }

    private Task<TaskItem?> FindOwnedAsync(Guid projectId, Guid id) =>
        _db.TaskItems.FirstOrDefaultAsync(
            t => t.Id == id && t.ProjectId == projectId && t.OwnerId == _currentUser.UserId);

    private static TaskItemResponse ToResponse(TaskItem t) =>
        new(t.Id, t.Title, t.Description, t.Status, t.DueDate,
            t.ProjectId, t.CreatedAt, t.UpdatedAt);
}
