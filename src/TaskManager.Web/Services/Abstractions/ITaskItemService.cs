using TaskManager.Web.Common.Results;
using TaskManager.Web.Models.Dtos.TaskItems;

namespace TaskManager.Web.Services.Abstractions;

public interface ITaskItemService
{
    Task<Result<IReadOnlyList<TaskItemResponse>>> ListAsync(Guid projectId);
    Task<Result<TaskItemResponse>> GetByIdAsync(Guid projectId, Guid id);
    Task<Result<TaskItemResponse>> CreateAsync(Guid projectId, CreateTaskItemRequest request);
    Task<Result<TaskItemResponse>> UpdateAsync(Guid projectId, Guid id, UpdateTaskItemRequest request);
    Task<Result<bool>> DeleteAsync(Guid projectId, Guid id);
}
