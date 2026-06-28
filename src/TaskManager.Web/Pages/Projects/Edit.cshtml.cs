using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManager.Web.Models.Dtos.Projects;
using TaskManager.Web.Services.Abstractions;

namespace TaskManager.Web.Pages.Projects;

[Authorize]
public class EditProjectModel : PageModel
{
    private readonly IProjectService _projects;

    [BindProperty]
    public ProjectInput Input { get; set; } = new();

    public Guid ProjectId { get; private set; }

    public string? ErrorMessage { get; private set; }

    public EditProjectModel(IProjectService projects) => _projects = projects;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        if (!await CanAccessProjectAsync(id))
            return RedirectToPage("/Dashboard");

        // TODO: load existing project; populate Input from it, e.g.:
        // var result = await _projects.GetByIdAsync(id);
        var result = await _projects.GetByIdAsync(id);
        if (!result.IsSuccess) return RedirectToPage("/Dashboard");

        var p = result.Value!;
        Input = new ProjectInput { Name = p.Name, Description = p.Description, DueDate = p.DueDate };
        ProjectId = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        if (!await CanAccessProjectAsync(id))
            return RedirectToPage("/Dashboard");

        ProjectId = id;

        if (!ModelState.IsValid) return Page();

        // TODO: call project service to update the project, e.g.:
        // var result = await _projects.UpdateAsync(id, new UpdateProjectRequest(Input.Name, Input.Description, Input.DueDate));
        var result = await _projects.UpdateAsync(id,
            new UpdateProjectRequest(Input.Name, Input.Description, Input.DueDate));

        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error!.Message;
            return Page();
        }

        // Assumption: /Projects/Details/{id} not yet implemented.
        // When added: return RedirectToPage("/Projects/Details", new { id });
        return RedirectToPage("/Dashboard");
    }

    private async Task<bool> CanAccessProjectAsync(Guid id) =>
        (await _projects.GetByIdAsync(id)).IsSuccess;
}
