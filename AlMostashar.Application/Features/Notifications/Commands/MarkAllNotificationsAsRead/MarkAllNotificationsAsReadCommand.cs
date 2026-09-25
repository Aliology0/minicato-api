using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;

public record MarkAllNotificationsAsReadCommand(int? LastReadId) : IRequest<Result<int>>;
