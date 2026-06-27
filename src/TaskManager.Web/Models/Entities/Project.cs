namespace TaskManager.Web.Models.Entities;

public class Project
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string OwnerId { get; set; } = null!;
    public ApplicationUser Owner { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = [];
}
