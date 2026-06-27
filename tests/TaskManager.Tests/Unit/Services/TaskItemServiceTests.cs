using Microsoft.EntityFrameworkCore;
using TaskManager.Web.Common.Results;
using TaskManager.Web.Data;
using TaskManager.Web.Models.Dtos.TaskItems;
using TaskManager.Web.Models.Entities;
using TaskManager.Web.Services;
using TaskManager.Tests.TestHelpers;
using Xunit;

namespace TaskManager.Tests.Unit.Services;

public sealed class TaskItemServiceTests : IDisposable
{
    private readonly TaskCoDbContext _db;
    private readonly FakeCurrentUser _currentUser;
    private readonly TaskItemService _sut;

    public TaskItemServiceTests()
    {
        var options = new DbContextOptionsBuilder<TaskCoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new TaskCoDbContext(options);
        _currentUser = new FakeCurrentUser { UserId = "owner-a" };
        _sut = new TaskItemService(_db, _currentUser);
    }

    public void Dispose() => _db.Dispose();

    // ── helpers ───────────────────────────────────────────────────────────────

    private Project SeedProject(string ownerId, string name = "Project")
    {
        var project = new Project
        {
            Id = Guid.NewGuid(), Name = name, OwnerId = ownerId,
            CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow
        };
        _db.Projects.Add(project);
        _db.SaveChanges();
        return project;
    }

    private TaskItem SeedTask(Guid projectId, string ownerId, string title = "Task")
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(), Title = title, Status = TaskStatus.Todo,
            ProjectId = projectId, OwnerId = ownerId,
            CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow
        };
        _db.TaskItems.Add(task);
        _db.SaveChanges();
        return task;
    }

    // ── ListAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task ListAsync_OwnedProject_ReturnsItsTasksOnly()
    {
        var projectA = SeedProject("owner-a");
        var projectB = SeedProject("owner-b");
        SeedTask(projectA.Id, "owner-a", "My Task");
        SeedTask(projectB.Id, "owner-b", "Other Task");

        var result = await _sut.ListAsync(projectA.Id);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("My Task", result.Value![0].Title);
    }

    [Fact]
    public async Task ListAsync_OtherOwnerProject_ReturnsNotFound()
    {
        var project = SeedProject("owner-b");

        var result = await _sut.ListAsync(project.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NotFound, result.Error!.Code);
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_OwnedTask_ReturnsTask()
    {
        var project = SeedProject("owner-a");
        var task = SeedTask(project.Id, "owner-a");

        var result = await _sut.GetByIdAsync(project.Id, task.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(task.Id, result.Value!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WrongProject_ReturnsNotFound()
    {
        var projectA = SeedProject("owner-a");
        var projectB = SeedProject("owner-a");  // same owner, different project
        var task = SeedTask(projectB.Id, "owner-a");

        var result = await _sut.GetByIdAsync(projectA.Id, task.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NotFound, result.Error!.Code);
    }

    [Fact]
    public async Task GetByIdAsync_OtherOwnerTask_ReturnsNotFound()
    {
        var project = SeedProject("owner-b");
        var task = SeedTask(project.Id, "owner-b");

        var result = await _sut.GetByIdAsync(project.Id, task.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NotFound, result.Error!.Code);
    }

    // ── CreateAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_OwnedProject_SetsOwnerIdFromProject()
    {
        var project = SeedProject("owner-a");
        var request = new CreateTaskItemRequest("New Task");

        var result = await _sut.CreateAsync(project.Id, request);

        Assert.True(result.IsSuccess);
        var stored = await _db.TaskItems.FindAsync(result.Value!.Id);
        Assert.Equal(project.OwnerId, stored!.OwnerId);
    }

    [Fact]
    public async Task CreateAsync_OtherOwnerProject_ReturnsNotFound()
    {
        var project = SeedProject("owner-b");

        var result = await _sut.CreateAsync(project.Id, new CreateTaskItemRequest("Hijack"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NotFound, result.Error!.Code);
    }

    [Fact]
    public async Task CreateAsync_SetsProjectIdFromRoute()
    {
        var project = SeedProject("owner-a");

        var result = await _sut.CreateAsync(project.Id, new CreateTaskItemRequest("Task"));

        Assert.True(result.IsSuccess);
        Assert.Equal(project.Id, result.Value!.ProjectId);
    }

    // ── UpdateAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_OwnedTask_UpdatesAndReturns()
    {
        var project = SeedProject("owner-a");
        var task = SeedTask(project.Id, "owner-a", "Old");

        var result = await _sut.UpdateAsync(project.Id, task.Id,
            new UpdateTaskItemRequest("New", "Desc", TaskStatus.Done));

        Assert.True(result.IsSuccess);
        Assert.Equal("New", result.Value!.Title);
        Assert.Equal(TaskStatus.Done, result.Value!.Status);
    }

    [Fact]
    public async Task UpdateAsync_OtherOwnerTask_ReturnsNotFound()
    {
        var project = SeedProject("owner-b");
        var task = SeedTask(project.Id, "owner-b");

        var result = await _sut.UpdateAsync(project.Id, task.Id,
            new UpdateTaskItemRequest("Hack"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NotFound, result.Error!.Code);
    }

    // ── DeleteAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_OwnedTask_RemovesAndReturnsTrue()
    {
        var project = SeedProject("owner-a");
        var task = SeedTask(project.Id, "owner-a");

        var result = await _sut.DeleteAsync(project.Id, task.Id);

        Assert.True(result.IsSuccess);
        Assert.Null(await _db.TaskItems.FindAsync(task.Id));
    }

    [Fact]
    public async Task DeleteAsync_OtherOwnerTask_ReturnsNotFound()
    {
        var project = SeedProject("owner-b");
        var task = SeedTask(project.Id, "owner-b");

        var result = await _sut.DeleteAsync(project.Id, task.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NotFound, result.Error!.Code);
    }
}
