using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.ValueObject.Enum;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AlMostashar.Infrastructure.Authorization;

public class LawyerApprovedHandler : AuthorizationHandler<LawyerApprovedRequirement>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public LawyerApprovedHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, LawyerApprovedRequirement requirement)
    {
        // Ensure role claim exists
        if (!context.User.IsInRole("Lawyer"))
        {
            return Task.CompletedTask;
        }

        // Prefer current user service if available to identify the user id
        int userId = _currentUser?.UserId ?? 0;
        if (userId == 0)
        {
            // Fallback: try to read sub claim
            var sub = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? context.User.FindFirst("sub")?.Value;
            if (!int.TryParse(sub, out userId))
                return Task.CompletedTask;
        }

        // Query persisted account status
        var lawyer = _db.Lawyers.Find(userId);
        if (lawyer == null)
            return Task.CompletedTask;

        if (lawyer.AccountStatus == AccountStatus.Active && lawyer.IsVerified)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
