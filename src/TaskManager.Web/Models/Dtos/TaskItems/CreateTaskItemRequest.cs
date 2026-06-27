namespace TaskManager.Web.Models.Dtos.TaskItems;

public sealed record CreateTaskItemRequest(
    string Title,
    string? Description = null,
    TaskStatus Status = TaskStatus.Todo,
    DateTimeOffset? DueDate = null);
