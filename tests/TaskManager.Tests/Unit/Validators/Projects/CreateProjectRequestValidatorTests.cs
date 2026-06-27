using TaskManager.Web.Models.Dtos.Projects;
using TaskManager.Web.Validators.Projects;
using Xunit;

namespace TaskManager.Tests.Unit.Validators.Projects;

public sealed class CreateProjectRequestValidatorTests
{
    private readonly CreateProjectRequestValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_ValidRequest_Passes()
    {
        var result = await _validator.ValidateAsync(new CreateProjectRequest("My Project", null));
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ValidateAsync_EmptyOrNullName_Fails(string? name)
    {
        var result = await _validator.ValidateAsync(new CreateProjectRequest(name!, null));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProjectRequest.Name));
    }

    [Fact]
    public async Task ValidateAsync_NameExceeds200Chars_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateProjectRequest(new string('x', 201), null));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProjectRequest.Name));
    }

    [Fact]
    public async Task ValidateAsync_DescriptionExceeds2000Chars_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateProjectRequest("Valid", new string('x', 2001)));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProjectRequest.Description));
    }
}
