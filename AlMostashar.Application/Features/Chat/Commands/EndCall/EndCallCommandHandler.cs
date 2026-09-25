using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Chat.Commands.EndCall
{
    public class EndCallCommandHandler
        : IRequestHandler<EndCallCommand, Result<string>>
    {
        private readonly IAppDbContext _dbContext;
        private readonly ICurrentUserService _currentUser;
        private readonly INotificationService _notificationService;

        public EndCallCommandHandler(
            IAppDbContext dbContext,
            ICurrentUserService currentUser,
            INotificationService notificationService)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
            _notificationService = notificationService;
        }

        public async Task<Result<string>> Handle(
            EndCallCommand request, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUser.UserId;

            // 1. Fetch the call session
            var callSession = await _dbContext.CallSessions
                .FirstOrDefaultAsync(cs => cs.Id == request.CallSessionId, cancellationToken);

            if (callSession is null)
                return Result<string>.Failure(
                    new Error("Call.NotFound", Messages.Call.NotFound));

            // 2. Verify the current user is a participant
            if (callSession.CallerId != currentUserId && callSession.ReceiverId != currentUserId)
                return Result<string>.Failure(
                    new Error("Auth.UnauthorizedAccess", Messages.Call.NotParticipant));

            // 3. Transition status to Completed
            try
            {
                callSession.UpdateStatus(CallStatus.Completed);
            }
            catch (InvalidOperationException ex)
            {
                return Result<string>.Failure(
                    new Error("Call.InvalidTransition", ex.Message));
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            // 4. Notify the OTHER participant (not the one who ended the call)
            int otherUserId = callSession.CallerId == currentUserId
                ? callSession.ReceiverId
                : callSession.CallerId;

            await _notificationService.SendCallEndedAsync(
                otherUserId, Messages.Call.Ended, cancellationToken);


            return Result<string>.Success(Messages.Call.Ended);
        }
    }
}
