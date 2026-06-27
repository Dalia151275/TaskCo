namespace TaskManager.Web.Models.Dtos.TaskItems;

public sealed record UpdateTaskItemRequest(
    string Title,
    string? Description = null,
    TaskStatus Status = TaskStatus.Todo,
    DateTimeOffset? DueDate = null);
