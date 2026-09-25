using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Application.Features.Chat.DTOs
{
    public record ChatMessagesDto(int MessageId, int SenderId,
        MessageType MessageType, string Content,
        bool IsSeen, DateTime SentAt,
        DateTime? SeenAt, int? DocumentId=null);
    
}
