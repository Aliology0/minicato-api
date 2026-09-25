using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static AlMostashar.Application.Helpers.UploadToStorage;

namespace AlMostashar.Application.Features.ClientProfile.Commands.EditProfile;

public class EditClientProfileCommandHandler : IRequestHandler<EditClientProfileCommand, Result<Unit>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IStorageService _storageService;

    public EditClientProfileCommandHandler(
        IAppDbContext context,
        ICurrentUserService currentUser,
        IStorageService storageService)
    {
        _context = context;
        _currentUser = currentUser;
        _storageService = storageService;
    }

    public async Task<Result<Unit>> Handle(EditClientProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            return Result<Unit>.Failure(new Error("User.NotFound", Messages.Generic.NotFound(nameof(user))));

        // Upload profile image if provided
        if (request.ProfileImage is not null)
        {
            var fileKey = await UploadPublicAsync(request.ProfileImage, _storageService);
            user.AvatarUrl = _storageService.GetPublicUrl(fileKey);
        }

        user.FirstName = request.FirstName ?? user.FirstName;
        user.LastName = request.LastName ?? user.LastName;
        user.FullName = $"{user.FirstName} {user.LastName}";

        await _context.SaveChangesAsync(cancellationToken);
        
        return Result<Unit>.Success(Unit.Value);
    }
}
