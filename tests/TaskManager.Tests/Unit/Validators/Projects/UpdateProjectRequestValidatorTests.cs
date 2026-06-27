using TaskManager.Web.Models.Dtos.Projects;
using TaskManager.Web.Validators.Projects;
using Xunit;

namespace TaskManager.Tests.Unit.Validators.Projects;

public sealed class UpdateProjectRequestValidatorTests
{
    private readonly UpdateProjectRequestValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_ValidRequest_Passes()
    {
        var result = await _validator.ValidateAsync(new UpdateProjectRequest("Renamed", "New desc"));
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_EmptyName_Fails()
    {
        var result = await _validator.ValidateAsync(new UpdateProjectRequest("", null));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProjectRequest.Name));
    }
}
