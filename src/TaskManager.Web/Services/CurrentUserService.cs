using System.Security.Claims;
using TaskManager.Web.Services.Abstractions;

namespace TaskManager.Web.Services;

public sealed class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public string UserId =>
        Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Current user is not authenticated.");

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
}
