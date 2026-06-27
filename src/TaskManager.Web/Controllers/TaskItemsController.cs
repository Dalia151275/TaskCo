using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Web.Common.Api;
using TaskManager.Web.Common.Results;
using TaskManager.Web.Models.Dtos.TaskItems;
using TaskManager.Web.Services.Abstractions;

namespace TaskManager.Web.Controllers;

[Route("api/projects/{projectId:guid}/tasks")]
[Authorize]
public sealed class TaskItemsController : ApiControllerBase
{
    private readonly ITaskItemService _taskItemService;
    private readonly IValidator<CreateTaskItemRequest> _createValidator;
    private readonly IValidator<UpdateTaskItemRequest> _updateValidator;

    public TaskItemsController(
        ITaskItemService taskItemService,
        IValidator<CreateTaskItemRequest> createValidator,
        IValidator<UpdateTaskItemRequest> updateValidator)
    {
        _taskItemService = taskItemService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<IActionResult> ListAsync(Guid projectId)
    {
        var result = await _taskItemService.ListAsync(projectId);
        return FromResult(result, data => OkEnvelope(data));
    }

    [HttpGet("{id:guid}", Name = "GetTaskItem")]
    public async Task<IActionResult> GetByIdAsync(Guid projectId, Guid id)
    {
        var result = await _taskItemService.GetByIdAsync(projectId, id);
        return FromResult(result, data => OkEnvelope(data));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(Guid projectId, [FromBody] CreateTaskItemRequest request)
    {
        var validation = await _createValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return MapError(new Error(ErrorCodes.Validation, "Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var result = await _taskItemService.CreateAsync(projectId, request);
        return FromResult(result, data =>
            CreatedEnvelope("GetTaskItem", new { projectId, id = data.Id }, data));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(
        Guid projectId, Guid id, [FromBody] UpdateTaskItemRequest request)
    {
        var validation = await _updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return MapError(new Error(ErrorCodes.Validation, "Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var result = await _taskItemService.UpdateAsync(projectId, id, request);
        return FromResult(result, data => OkEnvelope(data));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid projectId, Guid id)
    {
        var result = await _taskItemService.DeleteAsync(projectId, id);
        return FromResult(result, _ => OkEnvelope(true));
    }
}
