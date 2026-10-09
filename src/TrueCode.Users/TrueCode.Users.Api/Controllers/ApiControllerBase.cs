using Microsoft.AspNetCore.Mvc;
using TrueCode.SharedKernel.Results;

namespace TrueCode.Users.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Match<T>(
        Result<T> result,
        Func<T, IActionResult> onSuccess,
        int failureStatus = StatusCodes.Status400BadRequest) =>
        result.IsSuccess ? onSuccess(result.Data) : Problem(result, failureStatus);

    protected IActionResult Problem<T>(Result<T> result, int status)
    {
        var problem = ProblemDetailsFactory.CreateProblemDetails(HttpContext, status, detail: result.GetErrorsString());
        problem.Extensions["errors"] = result.Errors;
        return new ObjectResult(problem) { StatusCode = status };
    }
}
