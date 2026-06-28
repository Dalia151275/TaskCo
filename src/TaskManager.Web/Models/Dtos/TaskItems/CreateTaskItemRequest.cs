namespace TaskManager.Web.Models.Dtos.TaskItems;

public sealed record CreateTaskItemRequest(
    string Title,
    string? Description = null,
    TaskStatus Status = TaskStatus.Todo,
    Priority Priority = Priority.Low,
    DateTimeOffset? DueDate = null);
