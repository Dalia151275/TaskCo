using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManager.Web.Models.Dtos.Projects;
using TaskManager.Web.Services.Abstractions;

namespace TaskManager.Web.Pages.Projects;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly IProjectService _projects;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public ProjectResponse? Project { get; private set; }

    public DeleteModel(IProjectService projects) => _projects = projects;

    public async Task<IActionResult> OnGetAsync()
    {
        var result = await _projects.GetByIdAsync(Id);
        if (!result.IsSuccess) return RedirectToPage("/Projects/Index");

        Project = result.Value;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _projects.DeleteAsync(Id);
        return RedirectToPage("/Projects/Index");
    }
}
