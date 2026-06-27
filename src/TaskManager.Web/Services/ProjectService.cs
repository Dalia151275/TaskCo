using Microsoft.EntityFrameworkCore;
using TaskManager.Web.Common.Results;
using TaskManager.Web.Data;
using TaskManager.Web.Models.Dtos.Projects;
using TaskManager.Web.Models.Entities;
using TaskManager.Web.Services.Abstractions;

namespace TaskManager.Web.Services;

public sealed class ProjectService : IProjectService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ProjectService(AppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<ProjectResponse>>> ListAsync()
    {
        var list = await _db.Projects
            .Where(p => p.OwnerId == _currentUser.UserId)
            .OrderBy(p => p.Name)
            .Select(p => new ProjectResponse(p.Id, p.Name, p.Description, p.CreatedAt, p.UpdatedAt))
            .ToListAsync();

        return list.AsReadOnly();
    }

    public async Task<Result<ProjectResponse>> GetByIdAsync(Guid id)
    {
        var project = await FindOwnedAsync(id);
        if (project is null)
            return new Error(ErrorCodes.NotFound, "Project not found.");

        return ToResponse(project);
    }

    public async Task<Result<ProjectResponse>> CreateAsync(CreateProjectRequest request)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            OwnerId = _currentUser.UserId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        return ToResponse(project);
    }

    public async Task<Result<ProjectResponse>> UpdateAsync(Guid id, UpdateProjectRequest request)
    {
        var project = await FindOwnedAsync(id);
        if (project is null)
            return new Error(ErrorCodes.NotFound, "Project not found.");

        project.Name = request.Name;
        project.Description = request.Description;
        project.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        return ToResponse(project);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var project = await FindOwnedAsync(id);
        if (project is null)
            return new Error(ErrorCodes.NotFound, "Project not found.");

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();

        return true;
    }

    private Task<Project?> FindOwnedAsync(Guid id) =>
        _db.Projects.FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == _currentUser.UserId);

    private static ProjectResponse ToResponse(Project p) =>
        new(p.Id, p.Name, p.Description, p.CreatedAt, p.UpdatedAt);
}
