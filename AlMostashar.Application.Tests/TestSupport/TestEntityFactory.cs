using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Application.Tests.TestSupport;

public static class TestEntityFactory
{
    public static Admin CreateAdmin(string suffix = "1")
    {
        return new Admin
        {
            FirstName = "Admin",
            LastName = suffix,
            FullName = $"Admin {suffix}",
            Email = $"admin{suffix}@test.local",
            PasswordHash = "HASH::pass",
            CreatedAt = DateTime.UtcNow,
            AccountStatus = AccountStatus.Active,
            IsEmailVerified = true
        };
    }

    public static Client CreateClient(string suffix = "1", bool isEmailVerified = true, AccountStatus? status = null)
    {
        return new Client
        {
            FirstName = "Client",
            LastName = suffix,
            FullName = $"Client {suffix}",
            Email = $"client{suffix}@test.local",
            PasswordHash = "HASH::pass",
            CreatedAt = DateTime.UtcNow,
            IsEmailVerified = isEmailVerified,
            AccountStatus = status ?? (isEmailVerified ? AccountStatus.Active : AccountStatus.EmailVerificationRequired)
        };
    }

    public static Lawyer CreateLawyer(
        string suffix = "1",
        bool isVerified = true,
        AccountStatus? status = null)
    {
        return new Lawyer
        {
            FirstName = "Lawyer",
            LastName = suffix,
            FullName = $"Lawyer {suffix}",
            Email = $"lawyer{suffix}@test.local",
            PasswordHash = "HASH::pass",
            CreatedAt = DateTime.UtcNow,
            IsEmailVerified = true,
            IsVerified = isVerified,
            AccountStatus = status ?? (isVerified ? AccountStatus.Active : AccountStatus.PendingReview),
            PhoneNo = "01000000000",
            Governorate = "Cairo",
            City = "Nasr City",
            SyndicateId = 123 + int.Parse(suffix)
        };
    }

    public static LegalService CreateLegalService(int adminId, ServiceType serviceType, string suffix = "1")
    {
        var (entity, error) = LegalService.Create(
            title: $"Service {suffix}",
            summary: $"Summary {suffix}",
            fullDescription: $"Description {suffix}",
            serviceType: serviceType,
            adminId: adminId);

        if (error is not null || entity is null)
            throw new InvalidOperationException($"Failed to create legal service: {error?.Code}");

        return entity;
    }

    public static LawyerService CreateLawyerService(int lawyerId, int legalServiceId, decimal price = 100m)
    {
        var (entity, error) = LawyerService.Create(lawyerId, legalServiceId, price, "30 minutes");
        if (error is not null || entity is null)
            throw new InvalidOperationException($"Failed to create lawyer service: {error?.Code}");

        return entity;
    }
}

