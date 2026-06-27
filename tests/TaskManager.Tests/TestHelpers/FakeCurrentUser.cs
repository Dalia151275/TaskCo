using TaskManager.Web.Services.Abstractions;

namespace TaskManager.Tests.TestHelpers;

public sealed class FakeCurrentUser : ICurrentUser
{
    public string UserId { get; set; } = "user-a";
    public bool IsAuthenticated { get; set; } = true;
}
