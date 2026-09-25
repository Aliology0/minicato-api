using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Locations;
using AlMostashar.Application.Features.Auth.Commands.RegisterLawyer;
using AlMostashar.Application.Features.ClientRequests.Commands.CreateBroadcastRequest;
using AlMostashar.Application.Features.ClientRequests.Commands.CreateDirectRequest;
using AlMostashar.Application.Features.ClientRequests.Queries.GetLawyersByService;
using AlMostashar.Application.Tests.TestSupport;
using AlMostashar.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Tests;

public class GovernorateValidationTests
{
    [Fact]
    public void CreateBroadcastRequest_InvalidGovernorateId_FailsValidation()
    {
        var validator = new CreateBroadcastRequestCommandValidator();
        var result = validator.Validate(new CreateBroadcastRequestCommand
        {
            LegalServiceId = 1,
            Title = "Broadcast request",
            ProblemDetails = "details",
            GovernorateId = 0,
            CityId = 102,
            Budget = 500m
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateBroadcastRequestCommand.GovernorateId));
    }

    [Fact]
    public void CreateBroadcastRequest_InvalidCityId_FailsValidation()
    {
        var validator = new CreateBroadcastRequestCommandValidator();
        var result = validator.Validate(new CreateBroadcastRequestCommand
        {
            LegalServiceId = 1,
            Title = "Broadcast request",
            ProblemDetails = "details",
            GovernorateId = 1,
            CityId = 0,
            Budget = 500m
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateBroadcastRequestCommand.CityId));
    }

    [Fact]
    public void CreateDirectRequest_InvalidGovernorateId_FailsValidation()
    {
        var validator = new CreateDirectRequestCommandValidator();
        var result = validator.Validate(new CreateDirectRequestCommand
        {
            LawyerId = 1,
            LegalServiceId = 1,
            Title = "Direct request",
            ProblemDetails = "details",
            GovernorateId = 0,
            CityId = 102
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateDirectRequestCommand.GovernorateId));
    }

    [Fact]
    public void CreateDirectRequest_InvalidCityId_FailsValidation()
    {
        var validator = new CreateDirectRequestCommandValidator();
        var result = validator.Validate(new CreateDirectRequestCommand
        {
            LawyerId = 1,
            LegalServiceId = 1,
            Title = "Direct request",
            ProblemDetails = "details",
            GovernorateId = 1,
            CityId = 0
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateDirectRequestCommand.CityId));
    }

    [Fact]
    public void RegisterLawyer_InvalidGovernorate_FailsValidation()
    {
        var validator = new RegisterLawyerCommandValidator();
        var result = validator.Validate(new RegisterLawyerCommand
        {
            FirstName = "Lawyer",
            LastName = "One",
            Email = "lawyer@test.local",
            Password = "Password1",
            PhoneNo = "01000000000",
            GovernorateId = 0,
            CityId = 0,
            SyndicateId = 123
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void GetLawyersByService_InvalidGovernorateId_FailsValidation()
    {
        var validator = new GetLawyersByServiceQueryValidator();
        var result = validator.Validate(new GetLawyersByServiceQuery(1, 0, 102));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetLawyersByServiceQuery.GovernorateId));
    }

    [Fact]
    public void GetLawyersByService_InvalidCityId_FailsValidation()
    {
        var validator = new GetLawyersByServiceQueryValidator();
        var result = validator.Validate(new GetLawyersByServiceQuery(1, 1, 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetLawyersByServiceQuery.CityId));
    }

    [Fact]
    public async Task RegisterLawyer_ValidCanonicalGovernorate_IsTrimmedAndStored()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var auth = new StubAuthService();
        var Loc = TestLocationCatalog.Instance;

        var handler = new RegisterLawyerCommandHandler(db, auth, Loc);
        var result = await handler.Handle(new RegisterLawyerCommand
        {
            FirstName = "Lawyer",
            LastName = "One",
            Email = "lawyer-governorate@test.local",
            Password = "Password1",
            PhoneNo = "01000000000",
            GovernorateId = 1,
            CityId = 1,
            SyndicateId = 123
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);

        var savedLawyer = await db.Lawyers.SingleAsync(l => l.Email == "lawyer-governorate@test.local");
        Assert.Equal("القاهرة", savedLawyer.Governorate);
    }

    [Fact]
    public async Task GetLawyersByService_UnknownGovernorateId_ReturnsFailure()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var handler = new GetLawyersByServiceQueryHandler(db, TestLocationCatalog.Instance);
        var result = await handler.Handle(new GetLawyersByServiceQuery(1, 999, null), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Validation.GovernorateLookupNotFound", result.Error?.Code);
    }

    [Fact]
    public async Task GetLawyersByService_UnknownCityId_ReturnsFailure()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var handler = new GetLawyersByServiceQueryHandler(db, TestLocationCatalog.Instance);
        var result = await handler.Handle(new GetLawyersByServiceQuery(1, 1, 999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Validation.CityLookupNotFound", result.Error?.Code);
    }

}
