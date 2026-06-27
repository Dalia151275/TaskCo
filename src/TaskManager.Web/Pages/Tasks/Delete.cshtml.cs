using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManager.Web.Models.Dtos.TaskItems;
using TaskManager.Web.Services.Abstractions;

namespace TaskManager.Web.Pages.Tasks;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly ITaskItemService _tasks;

    [BindProperty(SupportsGet = true)]
    public Guid ProjectId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public TaskItemResponse? Task { get; private set; }

    public DeleteModel(ITaskItemService tasks) => _tasks = tasks;

    public async Task<IActionResult> OnGetAsync()
    {
        var result = await _tasks.GetByIdAsync(ProjectId, Id);
        if (!result.IsSuccess)
            return RedirectToPage("/Tasks/Index", new { projectId = ProjectId });

        Task = result.Value;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _tasks.DeleteAsync(ProjectId, Id);
        return RedirectToPage("/Tasks/Index", new { projectId = ProjectId });
    }
}
