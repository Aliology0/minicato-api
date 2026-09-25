using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.ClientProfile.Commands.ChangePassword;

public class ChangePasswordCommand : IRequest<Result<Unit>>
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
