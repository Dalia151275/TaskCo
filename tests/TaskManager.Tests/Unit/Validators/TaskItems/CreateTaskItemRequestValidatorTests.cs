using TaskManager.Web.Models.Dtos.TaskItems;
using TaskManager.Web.Models.Entities;
using TaskManager.Web.Validators.TaskItems;
using Xunit;

namespace TaskManager.Tests.Unit.Validators.TaskItems;

public sealed class CreateTaskItemRequestValidatorTests
{
    private readonly CreateTaskItemRequestValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_ValidRequest_Passes()
    {
        var result = await _validator.ValidateAsync(
            new CreateTaskItemRequest("Fix bug", "Details", TaskStatus.InProgress, Priority.Low, null));
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ValidateAsync_EmptyOrNullTitle_Fails(string? title)
    {
        var result = await _validator.ValidateAsync(new CreateTaskItemRequest(title!));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTaskItemRequest.Title));
    }

    [Fact]
    public async Task ValidateAsync_TitleExceeds500Chars_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateTaskItemRequest(new string('x', 501)));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTaskItemRequest.Title));
    }

    [Fact]
    public async Task ValidateAsync_DescriptionExceeds5000Chars_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateTaskItemRequest("Title", new string('x', 5001)));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTaskItemRequest.Description));
    }

    [Fact]
    public async Task ValidateAsync_InvalidStatusValue_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateTaskItemRequest("Title", null, (TaskStatus)99));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTaskItemRequest.Status));
    }
}
