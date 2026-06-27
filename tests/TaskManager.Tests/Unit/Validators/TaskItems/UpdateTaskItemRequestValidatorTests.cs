using TaskManager.Web.Models.Dtos.TaskItems;
using TaskManager.Web.Models.Entities;
using TaskManager.Web.Validators.TaskItems;
using Xunit;

namespace TaskManager.Tests.Unit.Validators.TaskItems;

public sealed class UpdateTaskItemRequestValidatorTests
{
    private readonly UpdateTaskItemRequestValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_ValidRequest_Passes()
    {
        var result = await _validator.ValidateAsync(
            new UpdateTaskItemRequest("Updated title", null, TaskStatus.Done));
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_EmptyTitle_Fails()
    {
        var result = await _validator.ValidateAsync(new UpdateTaskItemRequest(""));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateTaskItemRequest.Title));
    }

    [Fact]
    public async Task ValidateAsync_InvalidStatus_Fails()
    {
        var result = await _validator.ValidateAsync(
            new UpdateTaskItemRequest("Title", null, (TaskStatus)99));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateTaskItemRequest.Status));
    }
}
