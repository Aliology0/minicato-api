using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Admin.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Admin.Commands.RegisterAdmin
{
    public class RegisterAdminCommandHandler : IRequestHandler<RegisterAdminCommand, Result<RegisterAdminResponseDto>>
    {
        private readonly IAppDbContext _db;
        private readonly IAuthService _authService;

        public RegisterAdminCommandHandler(IAppDbContext db, IAuthService authService)
        {
            _db = db;
            _authService = authService;
        }

        public async Task<Result<RegisterAdminResponseDto>> Handle(RegisterAdminCommand request, CancellationToken cancellationToken)
        {
            // 1. Reject if email is already registered (case-insensitive)
            bool emailExists = await _db.Users
                .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

            if (emailExists)
            {
                var error = new Error("User.Conflict", Messages.Auth.EmailAlreadyRegistered);
                return Result<RegisterAdminResponseDto>.Failure(error);
            }

            // 2. Build the Admin entity
            var admin = new Domain.Entities.Admin
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                FullName = $"{request.FirstName} {request.LastName}",
                Email = request.Email,
                IsEmailVerified = true, // Admin accounts created by another Admin are verified
                PasswordHash = _authService.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                AccountStatus = AccountStatus.Active
            };

            // 3. Persist admin to DB
            await _db.Admins.AddAsync(admin, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // 4. Generate Tokens
            var accessToken = _authService.GenerateJwtToken(admin.Id, admin.Email, admin.FullName, UserRole.Admin.ToString());
            var refreshTokenModel = _authService.GenerateRefreshToken();
            RefreshToken refreshToken = new RefreshToken { Token = refreshTokenModel, UserId = admin.Id, ExpiresAt= DateTime.UtcNow.AddDays(_authService.GetRefreshTokenExpiryDays()) };

            // 5. Save refresh token to database
            await _db.RefreshTokens.AddAsync(refreshToken, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // 6. Return response with tokens
            return Result<RegisterAdminResponseDto>.Success(new RegisterAdminResponseDto
            {
                FirstName= request.FirstName,
                LastName= request.LastName,
                UserId = admin.Id,
                Email = admin.Email,
                ProfileImage= admin.AvatarUrl,
                Role = UserRole.Admin,
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            });
        }
    }
}
