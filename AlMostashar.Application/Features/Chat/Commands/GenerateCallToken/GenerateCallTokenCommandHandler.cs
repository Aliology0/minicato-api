using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.Chat.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Chat.Commands.GenerateCallToken
{
    public class GenerateCallTokenCommandHandler
        : IRequestHandler<GenerateCallTokenCommand, Result<CallTokenResponse>>
    {
        private readonly IAppDbContext _dbContext;
        private readonly ICurrentUserService _currentUser;
        private readonly ICommunicationService _communicationService;
        private readonly INotificationService _notificationService;

        // Calls older than this are considered stale and will be auto-closed
        private const int StaleCallThresholdHours = 2;

        public GenerateCallTokenCommandHandler(
            IAppDbContext dbContext,
            ICurrentUserService currentUser,
            ICommunicationService CommunicationService,
            INotificationService notificationService)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
            _communicationService = CommunicationService;
            _notificationService = notificationService;
        }

        public async Task<Result<CallTokenResponse>> Handle(
            GenerateCallTokenCommand request, CancellationToken cancellationToken)
        {
            // 1. Get current user ID and calculate the time limit for active calls
            int currentUserId = _currentUser.UserId;
            var staleThreshold = DateTime.UtcNow.AddHours(-StaleCallThresholdHours);

            // 2. Fetch chat details from the database efficiently (read-only)
            var chatInfo = await _dbContext.Chats.AsNoTracking()
                .Where(c => c.Id == request.ChatId)
                .Select(c => new
                {
                    // Get basic info for all participants in this chat
                    Participants = c.ChatParticipants.Select(p => new { p.User.Id, p.User.FullName, p.User.AvatarUrl }).ToList(),

                    // Check if there is an active or ringing call right now
                    ActiveSession = c.CallSessions
                        .FirstOrDefault(cs => (cs.Status == CallStatus.Initiated || cs.Status == CallStatus.Ongoing)
                                           && cs.StartTime >= staleThreshold)
                })
                .FirstOrDefaultAsync(cancellationToken);

            // --- Validations ---

            // 3. Check if the chat exists in the database
            if (chatInfo is null)
                return Result<CallTokenResponse>.Failure(new Error("Chat.NotFound", Messages.Chat.NotFound));

            // 4. Check if the user making the request is actually part of this chat
            if (!chatInfo.Participants.Any(u => u.Id == currentUserId))
                return Result<CallTokenResponse>.Failure(new Error("Auth.Unauthorized", Messages.Auth.NotChatParticipant));

            // 5. Identify the sender (caller) and the receiver
            var receiver = chatInfo.Participants.FirstOrDefault(u => u.Id != currentUserId);
            var sender = chatInfo.Participants.FirstOrDefault(s => s.Id == currentUserId);

            if (receiver == null || sender == null)
                return Result<CallTokenResponse>.Failure(new Error("Call.InvalidChat", Messages.Chat.OtherParticipantNotFound));

            // --- Call Session Logic ---

            // Setup variables to hold the call details
            var receiverInfo = new ReceiverInfo(receiver.Id, receiver.FullName, receiver.AvatarUrl ?? "");
            string channelName;
            string callToken;
            int callSessionId;
            bool isNewSession = false;
            uint uid = (uint)currentUserId;

            // 6. Check if we need to join an existing call or start a new one
            if (chatInfo.ActiveSession is not null)
            {
                // Scenario A: An active call exists. The user is rejoining the call.
                channelName = chatInfo.ActiveSession.ChannelName;
                callSessionId = chatInfo.ActiveSession.Id;

                // Generate a token for the caller to join the existing Agora channel
                callToken = _communicationService.GenerateCallToken(channelName, uid);
            }
            else
            {
                // Scenario B: No active call. We must create a new one.
                isNewSession = true;

                // Create a unique channel name using the chat ID and a random Guid
                channelName = $"chat_{request.ChatId}_{Guid.NewGuid():N}";

                // Save the new call record to the database
                var callSession = CallSession.Create(
                    channelName, request.ChatId, currentUserId, receiver.Id, request.CallType);

                _dbContext.CallSessions.Add(callSession);
                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException)
                {
                    // A concurrent request already created an active call for this chat
                    // due to the unique filtered index IX_CallSessions_ChatId_ActiveCall.
                    return Result<CallTokenResponse>.Failure(new Error("Call.AlreadyActive", Messages.Call.InvalidTransition));
                }

                callSessionId = callSession.Id;

                // Generate a token for the caller to create and join the new Agora channel
                callToken = _communicationService.GenerateCallToken(channelName, uid);
            }

            // --- Notification Logic ---

            // 7. If this is a completely new call, we must ring the receiver's phone
            if (isNewSession)
            {
                // Generate a separate, secure token specifically for the receiver
                var receiverToken = _communicationService.GenerateCallToken(channelName, (uint)receiver.Id);
                var callerInfo = new CallerInfo(sender.Id, sender.FullName, sender.AvatarUrl ?? "");

                // Prepare the data payload that will be sent via Firebase (FCM)
                var incomingCall = new IncomingCallDto(callSessionId.ToString(), request.CallType.ToString(), channelName, receiverToken, callerInfo);

                // Trigger the push notification to wake up the receiver's app (CallKit/Native UI)
                await _notificationService.TriggerIncomingCallAsync(receiver.Id, incomingCall, cancellationToken);
            }

            // 8. Return the call details to the caller so their app can connect to Agora
            return Result<CallTokenResponse>.Success(
                    new CallTokenResponse(uid, callSessionId, request.CallType.ToString(), channelName, _communicationService.GetAppId(), callToken, receiverInfo));
        }
    }
}
