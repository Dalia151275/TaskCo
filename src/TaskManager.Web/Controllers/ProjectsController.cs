using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Web.Common.Api;
using TaskManager.Web.Common.Results;
using TaskManager.Web.Models.Dtos.Projects;
using TaskManager.Web.Services.Abstractions;

namespace TaskManager.Web.Controllers;

[Route("api/projects")]
[Authorize]
public sealed class ProjectsController : ApiControllerBase
{
    private readonly IProjectService _projectService;
    private readonly IValidator<CreateProjectRequest> _createValidator;
    private readonly IValidator<UpdateProjectRequest> _updateValidator;

    public ProjectsController(
        IProjectService projectService,
        IValidator<CreateProjectRequest> createValidator,
        IValidator<UpdateProjectRequest> updateValidator)
    {
        _projectService = projectService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<IActionResult> ListAsync()
    {
        var result = await _projectService.ListAsync();
        return FromResult(result, data => OkEnvelope(data));
    }

    [HttpGet("{id:guid}", Name = "GetProject")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var result = await _projectService.GetByIdAsync(id);
        return FromResult(result, data => OkEnvelope(data));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateProjectRequest request)
    {
        var validation = await _createValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return MapError(new Error(ErrorCodes.Validation, "Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var result = await _projectService.CreateAsync(request);
        return FromResult(result, data => CreatedEnvelope("GetProject", new { id = data.Id }, data));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateProjectRequest request)
    {
        var validation = await _updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return MapError(new Error(ErrorCodes.Validation, "Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var result = await _projectService.UpdateAsync(id, request);
        return FromResult(result, data => OkEnvelope(data));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var result = await _projectService.DeleteAsync(id);
        return FromResult(result, _ => OkEnvelope(true));
    }
}
