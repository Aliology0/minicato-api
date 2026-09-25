using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.ClientProfile.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.ClientProfile.Queries.GetProfile;

public class GetClientProfileQueryHandler : IRequestHandler<GetClientProfileQuery, Result<ClientProfileDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetClientProfileQueryHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<ClientProfileDto>> Handle(GetClientProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            return Result<ClientProfileDto>.Failure(new Error("User.NotFound", "User not found."));

        return Result<ClientProfileDto>.Success(new ClientProfileDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl
        });
    }
}
