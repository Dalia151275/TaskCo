using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TaskManager.Web.Models.Dtos.Projects;
using TaskManager.Web.Services.Abstractions;

namespace TaskManager.Web.Pages.Projects;

[Authorize]
public class EditModel : PageModel
{
    private readonly IProjectService _projects;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public EditModel(IProjectService projects) => _projects = projects;

    public async Task<IActionResult> OnGetAsync()
    {
        var result = await _projects.GetByIdAsync(Id);
        if (!result.IsSuccess) return RedirectToPage("/Projects/Index");

        Input = new InputModel { Name = result.Value!.Name, Description = result.Value.Description };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await _projects.UpdateAsync(Id, new UpdateProjectRequest(Input.Name!, Input.Description));
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
