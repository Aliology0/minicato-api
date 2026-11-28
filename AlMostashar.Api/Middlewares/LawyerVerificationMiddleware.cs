using System.Security.Claims;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.ValueObject.Enum;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Api.Middlewares;

public sealed class LawyerVerificationMiddleware
{
    private readonly RequestDelegate _next;

    public LawyerVerificationMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IAppDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated == true && context.User.IsInRole("Lawyer"))
        {
            var idValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirstValue("sub");
            if (!int.TryParse(idValue, out var lawyerId))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            var isVerified = await db.Lawyers.AsNoTracking()
                .AnyAsync(l => l.Id == lawyerId && l.IsVerified && l.AccountStatus == AccountStatus.Verified, context.RequestAborted);
            if (!isVerified && !IsAllowedWhileUnverified(context.Request, lawyerId))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    code = "Auth.LawyerVerificationRequired",
                    message = "This operation is available after admin verification."
                }, context.RequestAborted);
                return;
            }
        }

        await _next(context);
    }

    private static bool IsAllowedWhileUnverified(HttpRequest request, int lawyerId)
    {
        var path = request.Path;
        if (HttpMethods.IsPut(request.Method) &&
            (path.Equals("/api/lawyers/edit-profile", StringComparison.OrdinalIgnoreCase) ||
             path.Equals("/api/lawyers/change-password", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        if (!HttpMethods.IsGet(request.Method))
            return false;

        if (path.Equals("/api/lawyers", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/api/lawyers/specializations", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments is { Length: 4 } &&
            segments[0].Equals("api", StringComparison.OrdinalIgnoreCase) &&
            segments[1].Equals("lawyers", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(segments[2], out _) &&
            segments[3].Equals("profile", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (segments is not { Length: 5 } ||
            !segments[0].Equals("api", StringComparison.OrdinalIgnoreCase) ||
            !segments[1].Equals("lawyers", StringComparison.OrdinalIgnoreCase) ||
            !int.TryParse(segments[2], out var requestedLawyerId) || requestedLawyerId != lawyerId ||
            !segments[3].Equals("documents", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return segments[4].Equals("Ssn", StringComparison.OrdinalIgnoreCase) ||
               segments[4].Equals("SyndicateCard", StringComparison.OrdinalIgnoreCase);
    }
}
