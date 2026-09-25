using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.ClientProfile.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<Unit>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthService _authService;

    public ChangePasswordCommandHandler(IAppDbContext context, ICurrentUserService currentUser, IAuthService authService)
    {
        _context = context;
        _currentUser = currentUser;
        _authService = authService;
    }

    public async Task<Result<Unit>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            return Result<Unit>.Failure(new Error("User.NotFound", "User not found."));

        if (!_authService.VerifyPassword(user.PasswordHash, request.OldPassword))
        {
            return Result<Unit>.Failure(new Error("Auth.InvalidOldPassword", "The old password you entered is incorrect."));
        }

        user.PasswordHash = _authService.HashPassword(request.NewPassword);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
