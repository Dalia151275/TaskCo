using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;
using TaskManager.Web.Data;
using TaskManager.Web.Models.Entities;
using Xunit;

namespace TaskManager.Tests.TestHelpers;

/// <summary>
/// Base for integration test classes. Each subclass gets its own factory (and InMemory DB).
/// Use unique user IDs per test to keep data isolated without resetting the database.
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected CustomWebApplicationFactory Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

    public Task InitializeAsync()
    {
        Factory = new CustomWebApplicationFactory();
        Client = Factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
    }

    // ── DB seeding ────────────────────────────────────────────────────────────

    protected async Task<Project> SeedProjectAsync(string ownerId, string name = "Test Project")
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = name,
            OwnerId = ownerId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        return project;
    }

    protected async Task<TaskItem> SeedTaskItemAsync(
        string ownerId, Guid projectId, string title = "Test Task",
        TaskItemStatus status = TaskItemStatus.Todo)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            Status = status,
            ProjectId = projectId,
            OwnerId = ownerId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.TaskItems.Add(task);
        await db.SaveChangesAsync();
        return task;
    }

    // ── HTTP helpers ──────────────────────────────────────────────────────────

    protected HttpRequestMessage Request(HttpMethod method, string url, string userId,
        object? body = null)
    {
        var msg = new HttpRequestMessage(method, url);
        msg.Headers.Add(TestAuthHandler.UserIdHeader, userId);
        if (body is not null)
            msg.Content = JsonContent.Create(body);
        return msg;
    }

    // ── JSON assertion helpers ────────────────────────────────────────────────

    protected static JsonElement ParseData(string json)
    {
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("data", out var data),
            $"Expected 'data' property in response: {json}");
        return data;
    }

    protected static JsonElement ParseError(string json)
    {
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("error", out var error),
            $"Expected 'error' property in response: {json}");
        Assert.True(error.TryGetProperty("code", out _), $"Error missing 'code': {json}");
        Assert.True(error.TryGetProperty("message", out _), $"Error missing 'message': {json}");
        return error;
    }
}
