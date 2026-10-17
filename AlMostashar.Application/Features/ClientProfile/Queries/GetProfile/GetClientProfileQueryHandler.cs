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
    private readonly AlMostashar.Application.Common.Interfaces.IStorageService _storageService;

    public GetClientProfileQueryHandler(IAppDbContext context, ICurrentUserService currentUser, AlMostashar.Application.Common.Interfaces.IStorageService storageService)
    {
        _context = context;
        _currentUser = currentUser;
        _storageService = storageService;
    }

    public async Task<Result<ClientProfileDto>> Handle(GetClientProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            return Result<ClientProfileDto>.Failure(new Error("User.NotFound", "User not found.", null, "المستخدم غير موجود."));

        return Result<ClientProfileDto>.Success(new ClientProfileDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            AvatarUrl = AlMostashar.Application.Common.Helpers.StorageUrlResolver.ResolvePublicUrl(_storageService, user.AvatarUrl)
        });
    }
}
