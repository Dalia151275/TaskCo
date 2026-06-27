using System.Net;
using TaskManager.Tests.TestHelpers;
using Xunit;

namespace TaskManager.Tests.Integration;

/// <summary>
/// Proves that owner A always receives 404 (never 403 or 200) when accessing owner B's resources,
/// and that all error responses have the correct envelope shape.
/// </summary>
public sealed class OwnershipIsolationTests : IntegrationTestBase
{
    private const string UserA = "isolation-user-a";
    private const string UserB = "isolation-user-b";

    // ── Projects ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProject_BelongsToOtherOwner_Returns404WithErrorEnvelope()
    {
        var project = await SeedProjectAsync(UserB, "B's project");

        var response = await Client.SendAsync(
            Request(HttpMethod.Get, $"/api/projects/{project.Id}", UserA));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = ParseError(await response.Content.ReadAsStringAsync());
        Assert.Equal("NOT_FOUND", error.GetProperty("code").GetString());
    }

    [Fact]
    public async Task UpdateProject_BelongsToOtherOwner_Returns404WithErrorEnvelope()
    {
        var project = await SeedProjectAsync(UserB, "B's project");

        var response = await Client.SendAsync(
            Request(HttpMethod.Put, $"/api/projects/{project.Id}", UserA,
                new { name = "Hijacked", description = "" }));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = ParseError(await response.Content.ReadAsStringAsync());
        Assert.Equal("NOT_FOUND", error.GetProperty("code").GetString());
    }

    [Fact]
    public async Task DeleteProject_BelongsToOtherOwner_Returns404WithErrorEnvelope()
    {
        var project = await SeedProjectAsync(UserB, "B's project");

        var response = await Client.SendAsync(
            Request(HttpMethod.Delete, $"/api/projects/{project.Id}", UserA));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = ParseError(await response.Content.ReadAsStringAsync());
        Assert.Equal("NOT_FOUND", error.GetProperty("code").GetString());
    }

    [Fact]
    public async Task ListProjects_DoesNotExposeOtherOwnersProjects()
    {
        await SeedProjectAsync(UserB, "B's secret project");

        var response = await Client.SendAsync(
            Request(HttpMethod.Get, "/api/projects", UserA));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var data = ParseData(await response.Content.ReadAsStringAsync());
        Assert.Equal(0, data.GetArrayLength());
    }

    [Fact]
    public async Task SuccessEnvelope_HasDataProperty_NotErrorProperty()
    {
        var project = await SeedProjectAsync(UserA, "A's project");

        var response = await Client.SendAsync(
            Request(HttpMethod.Get, $"/api/projects/{project.Id}", UserA));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = System.Text.Json.JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        Assert.True(json.RootElement.TryGetProperty("data", out _));
        Assert.False(json.RootElement.TryGetProperty("error", out _));
    }

    [Fact]
    public async Task ErrorEnvelope_HasErrorProperty_NotDataProperty()
    {
        var project = await SeedProjectAsync(UserB, "B's project");

        var response = await Client.SendAsync(
            Request(HttpMethod.Get, $"/api/projects/{project.Id}", UserA));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var json = System.Text.Json.JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        Assert.False(json.RootElement.TryGetProperty("data", out _));
        Assert.True(json.RootElement.TryGetProperty("error", out _));
    }

    // ── TaskItems ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTaskItem_BelongsToOtherOwner_Returns404WithErrorEnvelope()
    {
        var project = await SeedProjectAsync(UserB, "B's project");
        var task = await SeedTaskItemAsync(UserB, project.Id, "B's task");

        var response = await Client.SendAsync(
            Request(HttpMethod.Get, $"/api/projects/{project.Id}/tasks/{task.Id}", UserA));

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        var error = ParseError(await response.Content.ReadAsStringAsync());
        Assert.Equal("NOT_FOUND", error.GetProperty("code").GetString());
    }

    [Fact]
    public async Task UpdateTaskItem_BelongsToOtherOwner_Returns404WithErrorEnvelope()
    {
        var project = await SeedProjectAsync(UserB, "B's project");
        var task = await SeedTaskItemAsync(UserB, project.Id, "B's task");

        var response = await Client.SendAsync(
            Request(HttpMethod.Put, $"/api/projects/{project.Id}/tasks/{task.Id}", UserA,
                new { title = "Hijacked", status = "Done" }));

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        var error = ParseError(await response.Content.ReadAsStringAsync());
        Assert.Equal("NOT_FOUND", error.GetProperty("code").GetString());
    }

    [Fact]
    public async Task DeleteTaskItem_BelongsToOtherOwner_Returns404WithErrorEnvelope()
    {
        var project = await SeedProjectAsync(UserB, "B's project");
        var task = await SeedTaskItemAsync(UserB, project.Id, "B's task");

        var response = await Client.SendAsync(
            Request(HttpMethod.Delete, $"/api/projects/{project.Id}/tasks/{task.Id}", UserA));

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        var error = ParseError(await response.Content.ReadAsStringAsync());
        Assert.Equal("NOT_FOUND", error.GetProperty("code").GetString());
    }

    [Fact]
    public async Task CreateTaskItem_InOtherOwnersProject_Returns404WithErrorEnvelope()
    {
        var project = await SeedProjectAsync(UserB, "B's project");

        var response = await Client.SendAsync(
            Request(HttpMethod.Post, $"/api/projects/{project.Id}/tasks", UserA,
                new { title = "Injected task" }));

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        var error = ParseError(await response.Content.ReadAsStringAsync());
        Assert.Equal("NOT_FOUND", error.GetProperty("code").GetString());
    }

    [Fact]
    public async Task ListTaskItems_InOtherOwnersProject_Returns404WithErrorEnvelope()
    {
        var project = await SeedProjectAsync(UserB, "B's project");

        var response = await Client.SendAsync(
            Request(HttpMethod.Get, $"/api/projects/{project.Id}/tasks", UserA));

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        var error = ParseError(await response.Content.ReadAsStringAsync());
        Assert.Equal("NOT_FOUND", error.GetProperty("code").GetString());
    }
}
