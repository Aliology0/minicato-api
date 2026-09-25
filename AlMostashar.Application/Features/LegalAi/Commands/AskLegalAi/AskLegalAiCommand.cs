using AlMostashar.Application.Features.LegalAi.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.LegalAi.Commands.AskLegalAi;

public sealed record AskLegalAiCommand(string Query) : IRequest<Result<LegalAiChatResponse>>;
