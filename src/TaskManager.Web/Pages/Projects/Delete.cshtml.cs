using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManager.Web.Models.Dtos.Projects;
using TaskManager.Web.Services.Abstractions;

namespace TaskManager.Web.Pages.Projects;

[Authorize]
public class DeleteProjectModel : PageModel
{
    private readonly IProjectService _projects;

    // Assumption: spec says public Project Project (entity) but CLAUDE.md prohibits
    // entities in pages; using ProjectResponse from the service layer instead.
    public ProjectResponse? Project { get; private set; }

    public DeleteProjectModel(IProjectService projects) => _projects = projects;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        if (!await CanAccessProjectAsync(id))
            return RedirectToPage("/Dashboard");

        // TODO: load project to display its name in the confirmation, e.g.:
        // var result = await _projects.GetByIdAsync(id);
        var result = await _projects.GetByIdAsync(id);
        if (!result.IsSuccess) return RedirectToPage("/Dashboard");

        Project = result.Value;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        if (!await CanAccessProjectAsync(id))
            return RedirectToPage("/Dashboard");

        // TODO: call project service to delete the project, e.g.:
        // await _projects.DeleteAsync(id);
        await _projects.DeleteAsync(id);
        return RedirectToPage("/Dashboard");
    }

    private async Task<bool> CanAccessProjectAsync(Guid id) =>
        (await _projects.GetByIdAsync(id)).IsSuccess;
}
