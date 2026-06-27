using Microsoft.EntityFrameworkCore;
using TaskManager.Web.Common.Results;
using TaskManager.Web.Data;
using TaskManager.Web.Models.Dtos.Projects;
using TaskManager.Web.Models.Entities;
using TaskManager.Web.Services;
using TaskManager.Tests.TestHelpers;
using Xunit;

namespace TaskManager.Tests.Unit.Services;

public sealed class ProjectServiceTests : IDisposable
{
    private readonly TaskCoDbContext _db;
    private readonly FakeCurrentUser _currentUser;
    private readonly ProjectService _sut;

    public ProjectServiceTests()
    {
        var options = new DbContextOptionsBuilder<TaskCoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new TaskCoDbContext(options);
        _currentUser = new FakeCurrentUser { UserId = "owner-a" };
        _sut = new ProjectService(_db, _currentUser);
    }

    public void Dispose() => _db.Dispose();

    // ── helpers ──────────────────────────────────────────────────────────────

    private Project Seed(string ownerId, string name = "Project")
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = name,
            OwnerId = ownerId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _db.Projects.Add(project);
        _db.SaveChanges();
        return project;
    }

    // ── ListAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ListAsync_ReturnsOnlyCurrentUserProjects()
    {
        Seed("owner-a", "Mine");
        Seed("owner-b", "Theirs");

        var result = await _sut.ListAsync();

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("Mine", result.Value![0].Name);
    }

    [Fact]
    public async Task ListAsync_NoProjects_ReturnsEmptyList()
    {
        var result = await _sut.ListAsync();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_OwnedProject_ReturnsProject()
    {
        var project = Seed("owner-a");

        var result = await _sut.GetByIdAsync(project.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(project.Id, result.Value!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_OtherOwnerProject_ReturnsNotFound()
    {
        var project = Seed("owner-b");

        var result = await _sut.GetByIdAsync(project.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NotFound, result.Error!.Code);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNotFound()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NotFound, result.Error!.Code);
    }

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_SetsOwnerIdFromCurrentUser()
    {
        var result = await _sut.CreateAsync(new CreateProjectRequest("New", null));

        Assert.True(result.IsSuccess);
        var stored = await _db.Projects.FindAsync(result.Value!.Id);
        Assert.Equal("owner-a", stored!.OwnerId);
    }

    [Fact]
    public async Task CreateAsync_ReturnsProjectResponse()
    {
        var result = await _sut.CreateAsync(new CreateProjectRequest("Alpha", "Desc"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Alpha", result.Value!.Name);
        Assert.Equal("Desc", result.Value!.Description);
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_OwnedProject_UpdatesAndReturns()
    {
        var project = Seed("owner-a", "Old");

        var result = await _sut.UpdateAsync(project.Id, new UpdateProjectRequest("New", "Updated desc"));

        Assert.True(result.IsSuccess);
        Assert.Equal("New", result.Value!.Name);
        Assert.Equal("Updated desc", result.Value!.Description);
    }

    [Fact]
    public async Task UpdateAsync_OtherOwnerProject_ReturnsNotFound()
    {
        var project = Seed("owner-b");

        var result = await _sut.UpdateAsync(project.Id, new UpdateProjectRequest("Hack", null));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NotFound, result.Error!.Code);
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_OwnedProject_RemovesAndReturnsTrue()
    {
        var project = Seed("owner-a");

        var result = await _sut.DeleteAsync(project.Id);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.Null(await _db.Projects.FindAsync(project.Id));
    }

    [Fact]
    public async Task DeleteAsync_OtherOwnerProject_ReturnsNotFound()
    {
        var project = Seed("owner-b");

        var result = await _sut.DeleteAsync(project.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NotFound, result.Error!.Code);
    }
}
