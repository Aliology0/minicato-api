using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Notifications.Commands.RegisterFcmToken;

public class RegisterFcmTokenCommandHandler : IRequestHandler<RegisterFcmTokenCommand, Result<Unit>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RegisterFcmTokenCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(RegisterFcmTokenCommand request, CancellationToken cancellationToken)
    {
        var existing = await _db.UserFcmTokens
            .FirstOrDefaultAsync(t => t.Token == request.Token, cancellationToken);

        if (existing is not null)
        {
            // Token already registered — just refresh the timestamp and device type
            existing.DeviceType = request.DeviceType;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // New device token — insert
            var fcmToken = new UserFcmToken
            {
                UserId = _currentUser.UserId,
                Token = request.Token,
                DeviceType = request.DeviceType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _db.UserFcmTokens.AddAsync(fcmToken, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
