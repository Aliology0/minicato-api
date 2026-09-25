using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using AlMostashar.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AlMostashar.Infrastructure.Services;

public sealed class AdminSeedService
{
    private readonly IAppDbContext _db;
    private readonly IAuthService _authService;
    private readonly AdminSettings _settings;
    private readonly ILogger<AdminSeedService> _logger;

    public AdminSeedService(
        IAppDbContext db,
        IAuthService authService,
        IOptions<AdminSettings> settings,
        ILogger<AdminSeedService> logger)
    {
        _db = db;
        _authService = authService;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var email = _settings.Email?.Trim();
        var password = _settings.Password;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Admin seed skipped: AdminSettings:Email or AdminSettings:Password is not configured.");
            return;
        }

        var exists = await _db.Users
            .AnyAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);

        if (exists)
            return;

        var firstName = string.IsNullOrWhiteSpace(_settings.FirstName) ? "System" : _settings.FirstName.Trim();
        var lastName = string.IsNullOrWhiteSpace(_settings.LastName) ? "Admin" : _settings.LastName.Trim();

        var admin = new Admin
        {
            FirstName = firstName,
            LastName = lastName,
            FullName = $"{firstName} {lastName}",
            Email = email,
            PasswordHash = _authService.HashPassword(password),
            CreatedAt = DateTime.UtcNow,
            IsEmailVerified = true,
            IsActive = true,
            AccountStatus = AccountStatus.Active,
            VerificationStatus = VerificationStatus.Approved
        };

        await _db.Admins.AddAsync(admin, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded initial admin account for {AdminEmail}.", email);
    }
}
