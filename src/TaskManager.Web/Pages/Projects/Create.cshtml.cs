using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TaskManager.Web.Models.Dtos.Projects;
using TaskManager.Web.Services.Abstractions;

namespace TaskManager.Web.Pages.Projects;

[Authorize]
public class CreateModel : PageModel
{
    private readonly IProjectService _projects;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public CreateModel(IProjectService projects) => _projects = projects;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await _projects.CreateAsync(new CreateProjectRequest(Input.Name!, Input.Description));
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error!.Message;
            return Page();
        }

        return RedirectToPage("/Projects/Index");
    }

    public sealed class InputModel
    {
        [Required, MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }
    }
}
