using TaskManager.Web.Models.Entities;

namespace TaskManager.Web.Models.Dtos.TaskItems;

public sealed record TaskItemResponse(
    Guid Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    DateTimeOffset? DueDate,
    Guid ProjectId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
