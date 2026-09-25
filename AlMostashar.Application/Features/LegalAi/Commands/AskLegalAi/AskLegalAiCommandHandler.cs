using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Exceptions;
using AlMostashar.Application.Features.LegalAi.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.LegalAi.Commands.AskLegalAi;

public sealed class AskLegalAiCommandHandler : IRequestHandler<AskLegalAiCommand, Result<LegalAiChatResponse>>
{
    private readonly ILegalAiClient _legalAiClient;
    private readonly ICurrentUserService _currentUser;

    public AskLegalAiCommandHandler(ILegalAiClient legalAiClient, ICurrentUserService currentUser)
    {
        _legalAiClient = legalAiClient;
        _currentUser = currentUser;
    }

    public async Task<Result<LegalAiChatResponse>> Handle(AskLegalAiCommand request, CancellationToken cancellationToken)
    {
        _ = _currentUser.UserId;

        try
        {
            var response = await _legalAiClient.AskAsync(request.Query.Trim(), cancellationToken);
            return Result<LegalAiChatResponse>.Success(response);
        }
        catch (LegalAiServiceUnavailableException)
        {
            return Result<LegalAiChatResponse>.Failure(
                new Error("LegalAi.Unavailable", "Legal AI service is temporarily unavailable."));
        }
    }
}
