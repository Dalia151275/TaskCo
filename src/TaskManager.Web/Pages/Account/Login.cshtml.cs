using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TaskManager.Web.Models.Dtos.Account;
using TaskManager.Web.Services.Abstractions;

namespace TaskManager.Web.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly IAccountService _accounts;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string? ErrorMessage { get; set; }

    public LoginModel(IAccountService accounts) => _accounts = accounts;

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated ?? false)
            return RedirectToPage("/Projects/Index");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await _accounts.LoginAsync(new LoginRequest(Input.Email!, Input.Password!));
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error!.Message;
            return Page();
        }

        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            return Redirect(ReturnUrl);

        return RedirectToPage("/Projects/Index");
    }

    public sealed class InputModel
    {
        [Required, EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
