using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace TaskManager.Tests.TestHelpers;

/// <summary>
/// Authenticates every request using headers so isolation tests can impersonate different users
/// without spinning up separate factories.
/// </summary>
public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestAuth";
    public const string DefaultUserId = "test-user-1";
    public const string DefaultEmail = "alice@example.com";

    // Tests set these headers to impersonate a specific user.
    public const string UserIdHeader = "X-Test-UserId";
    public const string EmailHeader = "X-Test-Email";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Context.Request.Headers[UserIdHeader].FirstOrDefault() ?? DefaultUserId;
        var email = Context.Request.Headers[EmailHeader].FirstOrDefault() ?? DefaultEmail;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
