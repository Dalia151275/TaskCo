namespace TaskManager.Web.Models.Entities;

public enum TaskItemStatus
{
    Todo,
    InProgress,
    Done
}

public class TaskItem
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string OwnerId { get; set; } = null!;
    public ApplicationUser Owner { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
