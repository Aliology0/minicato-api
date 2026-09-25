using AlMostashar.Domain.ValueObject.Enum;
using System.ComponentModel.DataAnnotations;

namespace AlMostashar.Api.DTOs
{
    public record SendMessageRequest(
        [Required] int ChatId,
        [Required] int ReceiverId,
        [Required] string Content,
        [Required] MessageType MessageType,
        int? DocumentId = null
    );
}
