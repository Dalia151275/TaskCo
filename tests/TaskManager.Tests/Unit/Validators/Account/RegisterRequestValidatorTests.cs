using TaskManager.Web.Models.Dtos.Account;
using TaskManager.Web.Validators.Account;
using Xunit;

namespace TaskManager.Tests.Unit.Validators.Account;

public sealed class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_ValidRequest_Passes()
    {
        var request = new RegisterRequest("user@example.com", "Password1", "Password1");
        var result = await _validator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("not-an-email", "Password1", "Password1")]
    [InlineData("", "Password1", "Password1")]
    public async Task ValidateAsync_InvalidEmail_Fails(string email, string password, string confirm)
    {
        var result = await _validator.ValidateAsync(new RegisterRequest(email, password, confirm));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.Email));
    }

    [Theory]
    [InlineData("user@example.com", "short", "short")]
    [InlineData("user@example.com", "", "")]
    public async Task ValidateAsync_ShortOrEmptyPassword_Fails(string email, string password, string confirm)
    {
        var result = await _validator.ValidateAsync(new RegisterRequest(email, password, confirm));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public async Task ValidateAsync_PasswordMismatch_Fails()
    {
        var request = new RegisterRequest("user@example.com", "Password1", "Different1");
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.ConfirmPassword));
    }
}
