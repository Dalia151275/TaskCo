using TaskManager.Web.Common.Results;
using TaskManager.Web.Models.Dtos.Projects;

namespace TaskManager.Web.Services.Abstractions;

public interface IProjectService
{
    Task<Result<IReadOnlyList<ProjectResponse>>> ListAsync();
    Task<Result<ProjectResponse>> GetByIdAsync(Guid id);
    Task<Result<ProjectResponse>> CreateAsync(CreateProjectRequest request);
    Task<Result<ProjectResponse>> UpdateAsync(Guid id, UpdateProjectRequest request);
    Task<Result<bool>> DeleteAsync(Guid id);
}
