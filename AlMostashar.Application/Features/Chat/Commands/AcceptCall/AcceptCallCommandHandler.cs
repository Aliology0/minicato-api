using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Models;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Chat.Commands.AcceptCall
{
    public class AcceptCallCommandHandler
        : IRequestHandler<AcceptCallCommand, Result<IncomingCallDto>>
    {
        private readonly IAppDbContext _dbContext;
        private readonly ICurrentUserService _currentUser;
        private readonly INotificationService _notificationService;
        private readonly ICommunicationService _communicationService;

        public AcceptCallCommandHandler(
            IAppDbContext dbContext,
            ICurrentUserService currentUser,
            INotificationService notificationService,
            ICommunicationService communicationService)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
            _notificationService = notificationService;
            _communicationService = communicationService;
        }

        public async Task<Result<IncomingCallDto>> Handle(
            AcceptCallCommand request, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUser.UserId;

            // 1. Fetch the call session
            var callSession = await _dbContext.CallSessions
                .FirstOrDefaultAsync(cs => cs.Id == request.CallSessionId, cancellationToken);

            if (callSession is null)
                return Result<IncomingCallDto>.Failure(
                    new Error("Call.NotFound", Messages.Call.NotFound));

            // 2. Verify the current user is a participant
            if (callSession.CallerId != currentUserId && callSession.ReceiverId != currentUserId)
                return Result<IncomingCallDto>.Failure(
                    new Error("Auth.UnauthorizedAccess", Messages.Call.NotParticipant));

            // 3. Transition status to Ongoing
            try
            {
                callSession.UpdateStatus(CallStatus.Ongoing);
            }
            catch (InvalidOperationException ex)
            {
                return Result<IncomingCallDto>.Failure(
                    new Error("Call.InvalidTransition", ex.Message));
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            // 4. Notify the caller that the call was accepted
            await _notificationService.SendCallAcceptedAsync(
                callSession.CallerId, Messages.Call.Accepted, cancellationToken);
            var callToken = _communicationService.GenerateCallToken(callSession.ChannelName, (uint)currentUserId);
            // 5. Return the call info (CallerInfo is null as per requirements)
            var dto = new IncomingCallDto(
                callSession.Id.ToString(),
                callSession.CallType.ToString(),
                callSession.ChannelName,
                callToken,
                null!);

            return Result<IncomingCallDto>.Success(dto);
        }
    }
}
