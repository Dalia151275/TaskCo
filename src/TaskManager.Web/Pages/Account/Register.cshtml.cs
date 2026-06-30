using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TaskManager.Web.Models.Dtos.Account;
using TaskManager.Web.Services.Abstractions;

namespace TaskManager.Web.Pages.Account;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly IAccountService _accounts;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IList<string> ErrorMessages { get; private set; } = [];

    public RegisterModel(IAccountService accounts) => _accounts = accounts;

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated ?? false)
            return RedirectToPage("/Dashboard");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.Password != Input.ConfirmPassword)
            ModelState.AddModelError(nameof(Input.ConfirmPassword), "Passwords do not match.");

        if (!ModelState.IsValid) return Page();

        var result = await _accounts.RegisterAsync(
            new RegisterRequest(Input.Email!, Input.Password!, Input.ConfirmPassword!));

        if (!result.IsSuccess)
        {
            ErrorMessages = result.Error!.Details is string[] details
                ? details
                : [result.Error.Message];
            return Page();
        }

        return RedirectToPage("/Account/Login");
    }

    public sealed class InputModel
    {
        [Required, EmailAddress]
        public string? Email { get; set; }

        [Required, MinLength(8)]
        public string? Password { get; set; }

        [Required]
        public string? ConfirmPassword { get; set; }
    }
}
