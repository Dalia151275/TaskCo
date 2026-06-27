using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManager.Web.Common.Api;
using TaskManager.Web.Common.Results;
using TaskManager.Web.Models.Dtos.Account;
using TaskManager.Web.Services.Abstractions;

namespace TaskManager.Web.Controllers;

[Route("api/account")]
public sealed class AccountController : ApiControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AccountController(
        IAccountService accountService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _accountService = accountService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
    {
        var validation = await _registerValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return MapError(new Error(ErrorCodes.Validation, "Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var result = await _accountService.RegisterAsync(request);
        return FromResult(result, data => CreatedAtAction(nameof(MeAsync), null, new { data }));
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
    {
        var validation = await _loginValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return MapError(new Error(ErrorCodes.Validation, "Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var result = await _accountService.LoginAsync(request);
        return FromResult(result, data => OkEnvelope(data));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> LogoutAsync()
    {
        await _accountService.LogoutAsync();
        return OkEnvelope<object?>(null);
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult MeAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        return OkEnvelope(new AuthResponse(userId, email));
    }
}
