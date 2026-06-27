using Microsoft.AspNetCore.Mvc;
using TaskManager.Web.Common.Results;

namespace TaskManager.Web.Common.Api;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult OkEnvelope<T>(T data) => Ok(new { data });

    protected IActionResult CreatedEnvelope<T>(string routeName, object? routeValues, T data) =>
        CreatedAtRoute(routeName, routeValues, new { data });

    protected IActionResult CreatedEnvelope<T>(T data) =>
        StatusCode(StatusCodes.Status201Created, new { data });

    protected IActionResult FromResult<T>(Result<T> result, Func<T, IActionResult> onSuccess) =>
        result.IsSuccess ? onSuccess(result.Value!) : MapError(result.Error!);

    protected IActionResult MapError(Error error)
    {
        var status = error.Code switch
        {
            ErrorCodes.NotFound       => StatusCodes.Status404NotFound,
            ErrorCodes.Validation     => StatusCodes.Status400BadRequest,
            ErrorCodes.Unauthenticated => StatusCodes.Status401Unauthorized,
            ErrorCodes.Conflict       => StatusCodes.Status409Conflict,
            _                         => StatusCodes.Status500InternalServerError
        };
        return StatusCode(status, new
        {
            error = new { code = error.Code, message = error.Message, details = error.Details }
        });
    }
}
