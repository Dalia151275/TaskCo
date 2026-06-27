using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManager.Web.Models.Dtos.Projects;
using TaskManager.Web.Services.Abstractions;

namespace TaskManager.Web.Pages.Projects;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IProjectService _projects;

    public IReadOnlyList<ProjectResponse> Projects { get; private set; } = [];

    public IndexModel(IProjectService projects) => _projects = projects;

    public async Task OnGetAsync()
    {
        var result = await _projects.ListAsync();
        if (result.IsSuccess) Projects = result.Value!;
    }
}
