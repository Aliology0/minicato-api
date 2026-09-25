using AlMostashar.Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Helpers;

/// <summary>
/// Extension methods for the ControllerBase class to simplify returning standardized HTTP responses from Result objects.
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Maps a Result&lt;T&gt; to an appropriate IActionResult based on its state and error logic.
    /// </summary>
    public static IActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result)
    {
        if (result.IsSuccess)
            return controller.Ok(result);

        var code = result.Error!.Code;

        // 409 Conflict — duplicate resource (e.g. email already registered)
        if (code.Contains("Conflict", StringComparison.OrdinalIgnoreCase))
            return controller.Conflict(result);

        // 404 Not Found
        if (code.Contains("NotFound", StringComparison.OrdinalIgnoreCase))
            return controller.NotFound(result);

        if (code.Contains("Forbidden", StringComparison.OrdinalIgnoreCase))
            return controller.StatusCode(StatusCodes.Status403Forbidden, result);

        // 401 Unauthorized — authentication failures
        if (code.Contains("Auth", StringComparison.OrdinalIgnoreCase))
            return controller.Unauthorized(result);

        // Default: 400 Bad Request
        return controller.BadRequest(result);
    }
}
