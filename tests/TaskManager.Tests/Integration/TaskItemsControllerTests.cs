using System.Net;
using System.Text.Json;
using TaskManager.Tests.TestHelpers;
using TaskManager.Web.Models.Entities;
using Xunit;

namespace TaskManager.Tests.Integration;

public sealed class TaskItemsControllerTests : IntegrationTestBase
{
    private static string NewUserId() => Guid.NewGuid().ToString();

    // ── GET /api/projects/{projectId}/tasks ───────────────────────────────────

    [Fact]
    public async Task ListAsync_OwnedProject_Returns200WithDataEnvelope()
    {
        var userId = NewUserId();
        var project = await SeedProjectAsync(userId, "P");
        await SeedTaskItemAsync(userId, project.Id, "Task A");
        await SeedTaskItemAsync(userId, project.Id, "Task B");

        var response = await Client.SendAsync(
            Request(HttpMethod.Get, $"/api/projects/{project.Id}/tasks", userId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var data = ParseData(await response.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Array, data.ValueKind);
        Assert.Equal(2, data.GetArrayLength());
    }

    [Fact]
    public async Task ListAsync_OtherOwnerProject_Returns404WithErrorEnvelope()
    {
        var ownerA = NewUserId();
        var ownerB = NewUserId();
        var project = await SeedProjectAsync(ownerB, "B's project");

        var response = await Client.SendAsync(
            Request(HttpMethod.Get, $"/api/projects/{project.Id}/tasks", ownerA));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        ParseError(await response.Content.ReadAsStringAsync());
    }

    // ── GET /api/projects/{projectId}/tasks/{id} ──────────────────────────────

    [Fact]
    public async Task GetByIdAsync_OwnedTask_Returns200WithDataEnvelope()
    {
        var userId = NewUserId();
        var project = await SeedProjectAsync(userId);
        var task = await SeedTaskItemAsync(userId, project.Id, "My Task");

        var response = await Client.SendAsync(
            Request(HttpMethod.Get, $"/api/projects/{project.Id}/tasks/{task.Id}", userId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var data = ParseData(await response.Content.ReadAsStringAsync());
        Assert.Equal(task.Id.ToString(), data.GetProperty("id").GetString());
        Assert.Equal("My Task", data.GetProperty("title").GetString());
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_Returns404WithErrorEnvelope()
    {
        var userId = NewUserId();
        var project = await SeedProjectAsync(userId);

        var response = await Client.SendAsync(
            Request(HttpMethod.Get,
                $"/api/projects/{project.Id}/tasks/{Guid.NewGuid()}", userId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        ParseError(await response.Content.ReadAsStringAsync());
    }

    // ── POST /api/projects/{projectId}/tasks ──────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidRequest_Returns201WithDataEnvelope()
    {
        var userId = NewUserId();
        var project = await SeedProjectAsync(userId);
        var body = new { title = "Build feature", description = "Details", status = "InProgress" };

        var response = await Client.SendAsync(
            Request(HttpMethod.Post, $"/api/projects/{project.Id}/tasks", userId, body));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var data = ParseData(await response.Content.ReadAsStringAsync());
        Assert.Equal("Build feature", data.GetProperty("title").GetString());
        Assert.Equal("InProgress", data.GetProperty("status").GetString());
        Assert.True(response.Headers.Location is not null, "Location header must be set");
    }

    [Fact]
    public async Task CreateAsync_EmptyTitle_Returns400WithValidationErrorEnvelope()
    {
        var userId = NewUserId();
        var project = await SeedProjectAsync(userId);

        var response = await Client.SendAsync(
            Request(HttpMethod.Post, $"/api/projects/{project.Id}/tasks", userId,
                new { title = "" }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = ParseError(await response.Content.ReadAsStringAsync());
        Assert.Equal("VALIDATION", error.GetProperty("code").GetString());
    }

    // ── PUT /api/projects/{projectId}/tasks/{id} ──────────────────────────────

    [Fact]
    public async Task UpdateAsync_OwnedTask_Returns200WithUpdatedData()
    {
        var userId = NewUserId();
        var project = await SeedProjectAsync(userId);
        var task = await SeedTaskItemAsync(userId, project.Id, "Old Title");

        var response = await Client.SendAsync(
            Request(HttpMethod.Put, $"/api/projects/{project.Id}/tasks/{task.Id}", userId,
                new { title = "New Title", status = "Done" }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var data = ParseData(await response.Content.ReadAsStringAsync());
        Assert.Equal("New Title", data.GetProperty("title").GetString());
        Assert.Equal("Done", data.GetProperty("status").GetString());
    }

    // ── DELETE /api/projects/{projectId}/tasks/{id} ───────────────────────────

    [Fact]
    public async Task DeleteAsync_OwnedTask_Returns200WithTrueData()
    {
        var userId = NewUserId();
        var project = await SeedProjectAsync(userId);
        var task = await SeedTaskItemAsync(userId, project.Id);

        var response = await Client.SendAsync(
            Request(HttpMethod.Delete, $"/api/projects/{project.Id}/tasks/{task.Id}", userId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var data = ParseData(await response.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.True, data.ValueKind);
    }
}
