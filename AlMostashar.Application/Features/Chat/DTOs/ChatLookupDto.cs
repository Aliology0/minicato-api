using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlMostashar.Application.Features.Chat.DTOs
{
    public class ChatLookupDto
    {
        public ChatLookupDto(int chatId,
            int userId, string fullName,
            string? profileImage,
            string caseType, string? lastMessageContent, MessageType? lastMessageType, 
            DateTime? lastMessageDate, bool isLastMessageFromMe, int messagesCount)
        {
            ChatId = chatId;
            UserId = userId;
            FullName = fullName;
            ProfileImage = profileImage??"";
            CaseType = caseType;
            LastMessageContent = lastMessageContent;
            LastMessageDate = lastMessageDate;
            MessagesCount = messagesCount;
        }
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string ProfileImage { get; set; } 
        public string CaseType { get; set; }
        public string? LastMessageContent { get; set; } 
        public MessageType? LastMessageType { get; set; } 
        public DateTime? LastMessageDate { get; set; }
        public bool IsLastMessageFromMe { get; set; }
        public int? MessagesCount { get; set; }
    }
}
