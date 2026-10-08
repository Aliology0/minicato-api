using AlMostashar.Domain.Shared;
using AlMostashar.Application.Common.Constants;
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
        // Build standardized payload containing both English and Arabic messages
        object BuildPayload()
        {
            if (result.IsSuccess)
            {
                if (result.Value is string sv)
                {
                    return new
                    {
                        success = true,
                        message = sv,
                        messageAr = Messages.GetArabicForValue(sv),
                        value = result.Value
                    };
                }

                return new
                {
                    success = true,
                    message = (string?)null,
                    messageAr = (string?)null,
                    value = result.Value
                };
            }

            var err = result.Error!;
            var messageAr = err.MessageAr ?? Messages.GetArabicForValue(err.Message) ?? string.Empty;

            return new
            {
                success = false,
                message = err.Message,
                messageAr,
                code = err.Code,
                details = err.Details,
                value = result.Value
            };
        }

        var payload = BuildPayload();

        var codeStr = result.Error?.Code ?? string.Empty;

        // 409 Conflict — duplicate resource (e.g. email already registered)
        if (codeStr.Contains("Conflict", StringComparison.OrdinalIgnoreCase))
            return controller.Conflict(payload);

        // 404 Not Found
        if (codeStr.Contains("NotFound", StringComparison.OrdinalIgnoreCase))
            return controller.NotFound(payload);

        if (codeStr.Contains("Forbidden", StringComparison.OrdinalIgnoreCase))
            return controller.StatusCode(StatusCodes.Status403Forbidden, payload);

        // 401 Unauthorized — authentication failures
        if (codeStr.Contains("Auth", StringComparison.OrdinalIgnoreCase))
            return controller.Unauthorized(payload);

        // Default: 400 Bad Request
        return controller.BadRequest(payload);
    }
}
