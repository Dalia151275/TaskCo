using System.ComponentModel.DataAnnotations;

namespace TaskManager.Web.Pages.Projects;

public sealed class ProjectInput
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // Assumption: spec uses DateTime? but codebase uses DateTimeOffset throughout.
    public DateTimeOffset? DueDate { get; set; }
}
