using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManager.Web.Models.Dtos.Projects;
using TaskManager.Web.Services.Abstractions;

namespace TaskManager.Web.Pages.Projects;

[Authorize]
public class CreateProjectModel : PageModel
{
    private readonly IProjectService _projects;

    [BindProperty]
    public ProjectInput Input { get; set; } = new();

    public string? ErrorMessage { get; private set; }

    public CreateProjectModel(IProjectService projects) => _projects = projects;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        // TODO: call project service to create project for current user, e.g.:
        // var result = await _projects.CreateAsync(new CreateProjectRequest(Input.Name, Input.Description, Input.DueDate));
        var result = await _projects.CreateAsync(
            new CreateProjectRequest(Input.Name, Input.Description, Input.DueDate));

        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error!.Message;
            return Page();
        }

        return RedirectToPage("/Dashboard");
    }
}
