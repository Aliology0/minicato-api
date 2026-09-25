namespace AlMostashar.Application.Features.Chat.DTOs;

/// <summary>
/// Response returned when a call token is generated or an existing session is rejoined.
/// </summary>
public record CallTokenResponse(
    uint CallerId,
    int CallSessionId,
    string CallType,
    string ChannelName,
    string AppId,
    string Token,
    ReceiverInfo Receiver
);

public record ReceiverInfo(int ReceiverId, string ReceiverName, string ProfileImage);