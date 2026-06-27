using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TaskManager.Web.Models.Dtos.TaskItems;
using TaskManager.Web.Models.Entities;
using TaskManager.Web.Services.Abstractions;

namespace TaskManager.Web.Pages.Tasks;

[Authorize]
public class EditModel : PageModel
{
    private readonly ITaskItemService _tasks;

    [BindProperty(SupportsGet = true)]
    public Guid ProjectId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public EditModel(ITaskItemService tasks) => _tasks = tasks;

    public async Task<IActionResult> OnGetAsync()
    {
        var result = await _tasks.GetByIdAsync(ProjectId, Id);
        if (!result.IsSuccess)
            return RedirectToPage("/Tasks/Index", new { projectId = ProjectId });

        var task = result.Value!;
        Input = new InputModel
        {
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            DueDate = task.DueDate
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await _tasks.UpdateAsync(ProjectId, Id,
            new UpdateTaskItemRequest(Input.Title!, Input.Description, Input.Status, Input.DueDate));

        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error!.Message;
            return Page();
        }

        return RedirectToPage("/Tasks/Index", new { projectId = ProjectId });
    }

    public sealed class InputModel
    {
        [Required, MaxLength(500)]
        public string? Title { get; set; }

        [MaxLength(5000)]
        public string? Description { get; set; }

        public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;

        public DateTimeOffset? DueDate { get; set; }
    }
}
