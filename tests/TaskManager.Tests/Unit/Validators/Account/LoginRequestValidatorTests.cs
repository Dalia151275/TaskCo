using TaskManager.Web.Models.Dtos.Account;
using TaskManager.Web.Validators.Account;
using Xunit;

namespace TaskManager.Tests.Unit.Validators.Account;

public sealed class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_ValidRequest_Passes()
    {
        var result = await _validator.ValidateAsync(new LoginRequest("user@example.com", "anypassword"));
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("not-an-email", "password")]
    [InlineData("", "password")]
    public async Task ValidateAsync_InvalidEmail_Fails(string email, string password)
    {
        var result = await _validator.ValidateAsync(new LoginRequest(email, password));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginRequest.Email));
    }

    [Fact]
    public async Task ValidateAsync_EmptyPassword_Fails()
    {
        var result = await _validator.ValidateAsync(new LoginRequest("user@example.com", ""));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginRequest.Password));
    }
}
