using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace TaskManager.Tests.TestHelpers;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Test" environment causes Program.cs to register EF Core InMemory instead of SQLite,
        // keeping a single provider in the DI container.
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // Identity registers DefaultAuthenticateScheme/DefaultChallengeScheme explicitly;
            // Configure<AuthenticationOptions> here overrides them so [Authorize] uses TestAuth.
            services.Configure<AuthenticationOptions>(opts =>
            {
                opts.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                opts.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                opts.DefaultForbidScheme = TestAuthHandler.SchemeName;
            });
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });
        });
    }
}
