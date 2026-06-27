using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TaskManager.Web.Models.Dtos.TaskItems;
using TaskManager.Web.Models.Entities;
using TaskManager.Web.Services.Abstractions;

namespace TaskManager.Web.Pages.Tasks;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ITaskItemService _tasks;

    [BindProperty(SupportsGet = true)]
    public Guid ProjectId { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public CreateModel(ITaskItemService tasks) => _tasks = tasks;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await _tasks.CreateAsync(ProjectId,
            new CreateTaskItemRequest(Input.Title!, Input.Description, Input.Status, Input.DueDate));

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
