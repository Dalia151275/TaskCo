using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TaskManager.Tests.TestHelpers;
using Xunit;

namespace TaskManager.Tests.Integration;

public sealed class ProjectsControllerTests : IntegrationTestBase
{
    // Each test gets a unique user ID so seeded data never leaks across tests.
    private static string NewUserId() => Guid.NewGuid().ToString();

    // ── GET /api/projects ─────────────────────────────────────────────────────

    [Fact]
    public async Task ListAsync_AuthenticatedUser_Returns200WithDataEnvelope()
    {
        var userId = NewUserId();
        await SeedProjectAsync(userId, "Alpha");
        await SeedProjectAsync(userId, "Beta");
        await SeedProjectAsync(NewUserId(), "Other");   // different owner

        var response = await Client.SendAsync(Request(HttpMethod.Get, "/api/projects", userId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var data = ParseData(body);
        Assert.Equal(JsonValueKind.Array, data.ValueKind);
        Assert.Equal(2, data.GetArrayLength());
    }

    // ── GET /api/projects/{id} ────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_OwnedProject_Returns200WithDataEnvelope()
    {
        var userId = NewUserId();
        var project = await SeedProjectAsync(userId, "My Project");

        var response = await Client.SendAsync(
            Request(HttpMethod.Get, $"/api/projects/{project.Id}", userId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var data = ParseData(await response.Content.ReadAsStringAsync());
        Assert.Equal(project.Id.ToString(), data.GetProperty("id").GetString());
        Assert.Equal("My Project", data.GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_Returns404WithErrorEnvelope()
    {
        var userId = NewUserId();

        var response = await Client.SendAsync(
            Request(HttpMethod.Get, $"/api/projects/{Guid.NewGuid()}", userId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        ParseError(await response.Content.ReadAsStringAsync());
    }

    // ── POST /api/projects ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidRequest_Returns201WithDataEnvelope()
    {
        var userId = NewUserId();
        var body = new { name = "New Project", description = "Desc" };

        var response = await Client.SendAsync(
            Request(HttpMethod.Post, "/api/projects", userId, body));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var data = ParseData(await response.Content.ReadAsStringAsync());
        Assert.Equal("New Project", data.GetProperty("name").GetString());
        Assert.True(response.Headers.Location is not null, "Location header should be set");
    }

    [Fact]
    public async Task CreateAsync_EmptyName_Returns400WithErrorEnvelope()
    {
        var response = await Client.SendAsync(
            Request(HttpMethod.Post, "/api/projects", NewUserId(), new { name = "", description = "" }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = ParseError(await response.Content.ReadAsStringAsync());
        Assert.Equal("VALIDATION", error.GetProperty("code").GetString());
    }

    // ── PUT /api/projects/{id} ────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_OwnedProject_Returns200WithUpdatedData()
    {
        var userId = NewUserId();
        var project = await SeedProjectAsync(userId, "Original");

        var response = await Client.SendAsync(
            Request(HttpMethod.Put, $"/api/projects/{project.Id}", userId,
                new { name = "Renamed", description = "New" }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var data = ParseData(await response.Content.ReadAsStringAsync());
        Assert.Equal("Renamed", data.GetProperty("name").GetString());
    }

    // ── DELETE /api/projects/{id} ─────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_OwnedProject_Returns200WithTrueData()
    {
        var userId = NewUserId();
        var project = await SeedProjectAsync(userId);

        var response = await Client.SendAsync(
            Request(HttpMethod.Delete, $"/api/projects/{project.Id}", userId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var data = ParseData(await response.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.True, data.ValueKind);
    }
}
