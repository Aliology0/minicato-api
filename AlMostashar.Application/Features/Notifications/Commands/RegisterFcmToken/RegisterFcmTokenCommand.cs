using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Notifications.Commands.RegisterFcmToken;

public record RegisterFcmTokenCommand(
    string Token,
    string? DeviceType
) : IRequest<Result<Unit>>;
