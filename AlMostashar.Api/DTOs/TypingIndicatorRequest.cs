namespace AlMostashar.Api.DTOs
{
    public record TypingIndicatorRequest(
    int ChatId,
    int ReceiverId,
    bool IsTyping
    );
}
