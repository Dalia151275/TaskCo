using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManager.Web.Models.Dtos.Projects;
using TaskManager.Web.Models.Dtos.TaskItems;
using TaskManager.Web.Services.Abstractions;

namespace TaskManager.Web.Pages.Tasks;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ITaskItemService _tasks;
    private readonly IProjectService _projects;

    [BindProperty(SupportsGet = true)]
    public Guid ProjectId { get; set; }

    public ProjectResponse? Project { get; private set; }
    public IReadOnlyList<TaskItemResponse> Tasks { get; private set; } = [];

    public IndexModel(ITaskItemService tasks, IProjectService projects)
    {
        _tasks = tasks;
        _projects = projects;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var projectResult = await _projects.GetByIdAsync(ProjectId);
        if (!projectResult.IsSuccess) return RedirectToPage("/Projects/Index");

        Project = projectResult.Value;

        var tasksResult = await _tasks.ListAsync(ProjectId);
        if (tasksResult.IsSuccess) Tasks = tasksResult.Value!;

        return Page();
    }
}
