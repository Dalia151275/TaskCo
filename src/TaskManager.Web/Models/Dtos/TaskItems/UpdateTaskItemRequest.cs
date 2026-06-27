using TaskManager.Web.Models.Entities;

namespace TaskManager.Web.Models.Dtos.TaskItems;

public sealed record UpdateTaskItemRequest(
    string Title,
    string? Description = null,
    TaskItemStatus Status = TaskItemStatus.Todo,
    DateTimeOffset? DueDate = null);
