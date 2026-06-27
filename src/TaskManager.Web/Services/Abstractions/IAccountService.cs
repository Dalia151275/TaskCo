using TaskManager.Web.Common.Results;
using TaskManager.Web.Models.Dtos.Account;

namespace TaskManager.Web.Services.Abstractions;

public interface IAccountService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
    Task LogoutAsync();
}
