using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Lawyers.Commands.ChangePassword;

public sealed class ChangeLawyerPasswordCommandHandler : IRequestHandler<ChangeLawyerPasswordCommand, Result<Unit>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthService _authService;

    public ChangeLawyerPasswordCommandHandler(IAppDbContext db, ICurrentUserService currentUser, IAuthService authService)
    {
        _db = db;
        _currentUser = currentUser;
        _authService = authService;
    }

    public async Task<Result<Unit>> Handle(ChangeLawyerPasswordCommand request, CancellationToken cancellationToken)
    {
        if (!string.Equals(request.NewPassword, request.ConfirmNewPassword, StringComparison.Ordinal))
            return Result<Unit>.Failure(new Error("Lawyer.PasswordConfirmationMismatch", "New password and confirmation do not match."));

        if (!HasValidPasswordFormat(request.NewPassword))
            return Result<Unit>.Failure(new Error("Lawyer.PasswordPolicy", "New password must be at least 8 characters and contain uppercase, lowercase, and numeric characters."));

        var lawyer = await _db.Lawyers.FirstOrDefaultAsync(l => l.Id == _currentUser.UserId, cancellationToken);
        if (lawyer is null)
            return Result<Unit>.Failure(new Error("Lawyer.NotFound", "Lawyer not found."));

        if (!_authService.VerifyPassword(lawyer.PasswordHash, request.CurrentPassword))
            return Result<Unit>.Failure(new Error("Lawyer.CurrentPasswordInvalid", "The current password is incorrect."));

        lawyer.PasswordHash = _authService.HashPassword(request.NewPassword);
        await _db.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }

    private static bool HasValidPasswordFormat(string password) =>
        password.Length >= 8 && password.Any(char.IsUpper) && password.Any(char.IsLower) && password.Any(char.IsDigit);
}
