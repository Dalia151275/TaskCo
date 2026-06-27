using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManager.Web.Services.Abstractions;

namespace TaskManager.Web.Pages.Account;

[Authorize]
public class LogoutModel : PageModel
{
    private readonly IAccountService _accounts;

    public LogoutModel(IAccountService accounts) => _accounts = accounts;

    public IActionResult OnGet() => RedirectToPage("/Account/Login");

    public async Task<IActionResult> OnPostAsync()
    {
        await _accounts.LogoutAsync();
        return RedirectToPage("/Account/Login");
    }
}
