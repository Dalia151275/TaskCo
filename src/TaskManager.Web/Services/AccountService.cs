using Microsoft.AspNetCore.Identity;
using TaskManager.Web.Common.Results;
using TaskManager.Web.Models.Dtos.Account;
using TaskManager.Web.Models.Entities;
using TaskManager.Web.Services.Abstractions;

namespace TaskManager.Web.Services;

public sealed class AccountService : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var user = new ApplicationUser { Email = request.Email, UserName = request.Email };
        var identityResult = await _userManager.CreateAsync(user, request.Password);

        if (!identityResult.Succeeded)
        {
            var details = identityResult.Errors.Select(e => e.Description).ToArray();
            return new Error(ErrorCodes.Conflict, "Registration failed.", details);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return new AuthResponse(user.Id, user.Email!);
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var result = await _signInManager.PasswordSignInAsync(
            request.Email, request.Password, request.RememberMe, lockoutOnFailure: false);

        if (!result.Succeeded)
            return new Error(ErrorCodes.Unauthenticated, "Invalid email or password.");

        var user = await _userManager.FindByEmailAsync(request.Email);
        return new AuthResponse(user!.Id, user.Email!);
    }

    public async Task LogoutAsync() => await _signInManager.SignOutAsync();
}
