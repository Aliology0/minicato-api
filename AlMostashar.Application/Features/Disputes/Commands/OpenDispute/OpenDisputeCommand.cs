using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;

namespace AlMostashar.Application.Features.Disputes.Commands.OpenDispute;

public class OpenDisputeCommand : IRequest<Result<int>>
{
    [BindNever]
    public int CaseId { get; set; }

    public string Reason { get; set; } = string.Empty;
    public List<IFormFile>? Attachments { get; set; }
    public List<OpenDisputeChatMessageInput>? DisputeChatMessages { get; set; }
}

public class OpenDisputeChatMessageInput
{
    public int SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}

