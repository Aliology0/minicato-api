using AlMostashar.Application.Features.ClientRequests.Commands.CreateBroadcastRequest;
using AlMostashar.Application.Features.ClientRequests.Commands.CreateDirectRequest;
using AlMostashar.Application.Features.ClientRequests.Commands.SendOffer;
using AlMostashar.Application.Features.ClientRequests.Queries.GetBroadcastRequests;
using AlMostashar.Application.Features.ClientRequests.Queries.GetLawyersByService;
using AlMostashar.Application.Features.ClientRequests.Queries.GetMyRequests;
using AlMostashar.Application.Tests.TestSupport;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using AlMostashar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using AlMostashar.Application.Features.ClientRequests.RequestDetails;
using AlMostashar.Application.Features.ClientRequests.Services;
using AlMostashar.Api.Services;
using AlMostashar.Infrastructure.Services;
using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Application.Tests;

public class ClientRequestCreationAndQueryTests
{
    [Fact]
    public async Task CreateBroadcastRequest_WithActiveLegalService_StoresArabicRequestedLocationSnapshot()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var admin = TestEntityFactory.CreateAdmin("101");
        var client = TestEntityFactory.CreateClient("101");
        db.AddRange(admin, client);
        await db.SaveChangesAsync();

        var service = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Consultation, "101");
        db.LegalServices.Add(service);
        await db.SaveChangesAsync();

        var cairo = GetGovernorate("Cairo");
        var nasrCity = GetCity(cairo.Id, "Nasr City");
        var broadcastDocument = new CaseDocuments
        {
            DocumentName = "consultation.pdf",
            DocumentUrl = "documents/consultation",
            UploadedByUserId = client.Id,
            CreatedAt = DateTime.UtcNow
        };
        db.CaseDocuments.Add(broadcastDocument);
        await db.SaveChangesAsync();

        var handler = new CreateBroadcastRequestCommandHandler(db, new FixedCurrentUserService(client.Id), TestLocationCatalog.Instance, new RequestDetailsService(), new ClientRequestAttachmentService(db));
        var result = await handler.Handle(new CreateBroadcastRequestCommand
        {
            LegalServiceId = service.Id,
            Title = "Need legal consultation",
            ProblemDetails = "details",
            GovernorateId = cairo.Id,
            CityId = nasrCity.Id,
            Budget = 750m,
            AttachmentIds = [broadcastDocument.Id, broadcastDocument.Id],
            RequestDetails = JsonSerializer.SerializeToElement(new
            {
                legalBranch = "Civil",
                communicationMethod = "Video",
                consultationSummary = "Review the available documents"
            })
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);

        var saved = await db.BroadcastRequests.SingleAsync();
        Assert.Equal(service.Id, saved.LawyerServiceLegalServiceId);
        Assert.Equal(cairo.Id, saved.GovernorateId);
        Assert.Equal(cairo.Name, saved.Governorate);
        Assert.Equal(nasrCity.Id, saved.CityId);
        Assert.Equal(nasrCity.Name, saved.City);
        Assert.Equal(saved.Id, (await db.CaseDocuments.SingleAsync(d => d.Id == broadcastDocument.Id)).ClientRequestId);
    }

    [Fact]
    public async Task CreateBroadcastRequest_WhenGovernorateDoesNotExist_Fails()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var client = TestEntityFactory.CreateClient("102");
        db.Clients.Add(client);
        await db.SaveChangesAsync();

        var handler = new CreateBroadcastRequestCommandHandler(db, new FixedCurrentUserService(client.Id), TestLocationCatalog.Instance, new RequestDetailsService(), new ClientRequestAttachmentService(db));
        var result = await handler.Handle(new CreateBroadcastRequestCommand
        {
            LegalServiceId = 1,
            Title = "Need legal consultation",
            ProblemDetails = "details",
            GovernorateId = 9999,
            CityId = 39,
            Budget = 750m
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Validation.GovernorateLookupNotFound", result.Error?.Code);
    }

    [Fact]
    public async Task CreateBroadcastRequest_WhenCityDoesNotExist_Fails()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var client = TestEntityFactory.CreateClient("1021");
        db.Clients.Add(client);
        await db.SaveChangesAsync();

        var cairo = GetGovernorate("Cairo");
        var handler = new CreateBroadcastRequestCommandHandler(db, new FixedCurrentUserService(client.Id), TestLocationCatalog.Instance, new RequestDetailsService(), new ClientRequestAttachmentService(db));
        var result = await handler.Handle(new CreateBroadcastRequestCommand
        {
            LegalServiceId = 1,
            Title = "Need legal consultation",
            ProblemDetails = "details",
            GovernorateId = cairo.Id,
            CityId = 9999,
            Budget = 750m
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Validation.CityLookupNotFound", result.Error?.Code);
    }

    [Fact]
    public async Task CreateBroadcastRequest_WhenCityDoesNotBelongToGovernorate_Fails()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var client = TestEntityFactory.CreateClient("1022");
        db.Clients.Add(client);
        await db.SaveChangesAsync();

        var cairo = GetGovernorate("Cairo");
        var dokki = GetCity(2, "Dokki");

        var handler = new CreateBroadcastRequestCommandHandler(db, new FixedCurrentUserService(client.Id), TestLocationCatalog.Instance, new RequestDetailsService(), new ClientRequestAttachmentService(db));
        var result = await handler.Handle(new CreateBroadcastRequestCommand
        {
            LegalServiceId = 1,
            Title = "Need legal consultation",
            ProblemDetails = "details",
            GovernorateId = cairo.Id,
            CityId = dokki.Id,
            Budget = 750m
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Validation.CityGovernorateMismatch", result.Error?.Code);
    }

    [Fact]
    public async Task CreateBroadcastRequest_WhenLegalServiceDoesNotExist_Fails()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var client = TestEntityFactory.CreateClient("103");
        db.Clients.Add(client);
        await db.SaveChangesAsync();

        var cairo = GetGovernorate("Cairo");
        var nasrCity = GetCity(cairo.Id, "Nasr City");

        var handler = new CreateBroadcastRequestCommandHandler(db, new FixedCurrentUserService(client.Id), TestLocationCatalog.Instance, new RequestDetailsService(), new ClientRequestAttachmentService(db));
        var result = await handler.Handle(new CreateBroadcastRequestCommand
        {
            LegalServiceId = 999,
            Title = "Need legal consultation",
            ProblemDetails = "details",
            GovernorateId = cairo.Id,
            CityId = nasrCity.Id,
            Budget = 750m
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Request.InvalidLegalService", result.Error?.Code);
    }

    [Fact]
    public async Task CreateBroadcastRequest_WhenLegalServiceIsInactive_Fails()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var admin = TestEntityFactory.CreateAdmin("104");
        var client = TestEntityFactory.CreateClient("104");
        db.AddRange(admin, client);
        await db.SaveChangesAsync();

        var service = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Consultation, "104");
        service.Deactivate();
        db.LegalServices.Add(service);
        await db.SaveChangesAsync();

        var cairo = GetGovernorate("Cairo");
        var nasrCity = GetCity(cairo.Id, "Nasr City");

        var handler = new CreateBroadcastRequestCommandHandler(db, new FixedCurrentUserService(client.Id), TestLocationCatalog.Instance, new RequestDetailsService(), new ClientRequestAttachmentService(db));
        var result = await handler.Handle(new CreateBroadcastRequestCommand
        {
            LegalServiceId = service.Id,
            Title = "Need legal consultation",
            ProblemDetails = "details",
            GovernorateId = cairo.Id,
            CityId = nasrCity.Id,
            Budget = 750m
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Request.InvalidLegalService", result.Error?.Code);
    }

    //[Fact]
    //public async Task CreateDirectRequest_PersistsArabicLocationSnapshot_AndKeepsAssignedLawyerService()
    //{
    //    await using var scope = await TestDbContextScope.CreateAsync();
    //    var db = scope.DbContext;

    //    var admin = TestEntityFactory.CreateAdmin("105");
    //    var client = TestEntityFactory.CreateClient("105");
    //    var lawyer = TestEntityFactory.CreateLawyer("105");
    //    db.AddRange(admin, client, lawyer);
    //    await db.SaveChangesAsync();

    //    var service = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Contract, "105");
    //    db.LegalServices.Add(service);
    //    await db.SaveChangesAsync();

    //    db.LawyerServices.Add(TestEntityFactory.CreateLawyerService(lawyer.Id, service.Id, 500m));
    //    await db.SaveChangesAsync();

    //    var alexandria = GetGovernorate("Alexandria");
    //    var smouha = GetCity(alexandria.Id, "Smouha");
    //    var directDocument = new CaseDocuments
    //    {
    //        DocumentName = "contract.pdf",
    //        DocumentUrl = "documents/contract",
    //        UploadedByUserId = client.Id,
    //        CreatedAt = DateTime.UtcNow
    //    };
    //    db.CaseDocuments.Add(directDocument);
    //    await db.SaveChangesAsync();

    //    var handler = new CreateDirectRequestCommandHandler(db, new FixedCurrentUserService(client.Id), TestLocationCatalog.Instance, new RequestDetailsService(), new ClientRequestAttachmentService(db), new SignalRNotificationService(db, new InMemoryConnectionTracker(),new FcmNotificationService(new Logger<FcmNotificationService>()), new Logger<SignalRNotificationService>() ));
    //    var result = await handler.Handle(new CreateDirectRequestCommand
    //    {
    //        LawyerId = lawyer.Id,
    //        LegalServiceId = service.Id,
    //        Title = "Need contract review",
    //        ProblemDetails = "details",
    //        GovernorateId = alexandria.Id,
    //        CityId = smouha.Id,
    //        AttachmentIds = [directDocument.Id],
    //        RequestDetails = JsonSerializer.SerializeToElement(new
    //        {
    //            contractType = "Rental",
    //            contractRequestType = "Review",
    //            language = "Arabic",
    //            pagesCount = 8,
    //            allowedRevisions = 1
    //        })
    //    }, CancellationToken.None);

    //    Assert.True(result.IsSuccess);

    //    var saved = await db.DirectRequests.SingleAsync();
    //    Assert.Equal(service.Id, saved.LegalServiceId);
    //    Assert.Equal(ServiceType.Contract, saved.ServiceType);
    //    Assert.Equal(8, saved.ContractDetails!.PagesCount);
    //    Assert.Equal(alexandria.Id, saved.GovernorateId);
    //    Assert.Equal(alexandria.Name, saved.Governorate);
    //    Assert.Equal(smouha.Id, saved.CityId);
    //    Assert.Equal(smouha.Name, saved.City);
    //    Assert.Equal(lawyer.Id, saved.LawyerServiceLawyerId);
    //    Assert.Equal(service.Id, saved.LawyerServiceLegalServiceId);
    //    Assert.Equal(saved.Id, (await db.CaseDocuments.SingleAsync(d => d.Id == directDocument.Id)).ClientRequestId);
    //}

    //[Fact]
    //public async Task CreateDirectRequest_WhenCityDoesNotBelongToGovernorate_Fails()
    //{
    //    await using var scope = await TestDbContextScope.CreateAsync();
    //    var db = scope.DbContext;

    //    var client = TestEntityFactory.CreateClient("1051");
    //    db.Clients.Add(client);
    //    await db.SaveChangesAsync();

    //    var cairo = GetGovernorate("Cairo");
    //    var dokki = GetCity(2, "Dokki");

    //    var handler = new CreateDirectRequestCommandHandler(db, new FixedCurrentUserService(client.Id), TestLocationCatalog.Instance, new RequestDetailsService(), new ClientRequestAttachmentService(db));
    //    var result = await handler.Handle(new CreateDirectRequestCommand
    //    {
    //        LawyerId = 1,
    //        LegalServiceId = 1,
    //        Title = "Need contract review",
    //        ProblemDetails = "details",
    //        GovernorateId = cairo.Id,
    //        CityId = dokki.Id
    //    }, CancellationToken.None);

    //    Assert.False(result.IsSuccess);
    //    Assert.Equal("Validation.CityGovernorateMismatch", result.Error?.Code);
    //}

    [Fact]
    public async Task GetMyRequests_CountsOnlyPendingOffers()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var admin = TestEntityFactory.CreateAdmin("106");
        var client = TestEntityFactory.CreateClient("106");
        var lawyer1 = TestEntityFactory.CreateLawyer("106");
        var lawyer2 = TestEntityFactory.CreateLawyer("107");
        db.AddRange(admin, client, lawyer1, lawyer2);
        await db.SaveChangesAsync();

        var service = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.CompanyFormation, "106");
        db.LegalServices.Add(service);
        await db.SaveChangesAsync();

        db.LawyerServices.Add(TestEntityFactory.CreateLawyerService(lawyer1.Id, service.Id, 800m));
        db.LawyerServices.Add(TestEntityFactory.CreateLawyerService(lawyer2.Id, service.Id, 900m));
        await db.SaveChangesAsync();

        var cairo = GetGovernorate("Cairo");
        var nasrCity = GetCity(cairo.Id, "Nasr City");

        var request = new BroadcastRequest
        {
            RequestId = "REQ-Q-1",
            Title = "Need company setup",
            ProblemDetails = "details",
            GovernorateId = cairo.Id,
            Governorate = cairo.Name,
            CityId = nasrCity.Id,
            City = nasrCity.Name,
            Budget = 1000m,
            ClientId = client.Id,
            LawyerServiceLegalServiceId = service.Id,
            LegalServiceId = service.Id,
            ServiceType = service.ServiceType,
            Status = ClientRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        db.BroadcastRequests.Add(request);
        await db.SaveChangesAsync();

        db.RequestOffers.AddRange(
            new RequestOffer
            {
                ClientRequestId = request.Id,
                LawyerId = lawyer1.Id,
                LegalServiceId = service.Id,
                OfferedAmount = 950m,
                Status = OfferStatus.Pending,
                CreatedAt = DateTime.UtcNow
            },
            new RequestOffer
            {
                ClientRequestId = request.Id,
                LawyerId = lawyer2.Id,
                LegalServiceId = service.Id,
                OfferedAmount = 920m,
                Status = OfferStatus.Rejected,
                CreatedAt = DateTime.UtcNow
            });
        await db.SaveChangesAsync();

        var handler = new GetMyRequestsQueryHandler(db, new FixedCurrentUserService(client.Id));
        var result = await handler.Handle(new GetMyRequestsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!.Items);
        Assert.Equal(service.Title, dto.ServiceTitle);
        Assert.Equal(1, dto.OfferCount);
    }


    [Fact]
    public async Task SendOffer_ForBroadcastRequest_WithMismatchedLegalService_IsRejected()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var admin = TestEntityFactory.CreateAdmin("108");
        var client = TestEntityFactory.CreateClient("108");
        var lawyer = TestEntityFactory.CreateLawyer("108");
        db.AddRange(admin, client, lawyer);
        await db.SaveChangesAsync();

        var requestService = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Consultation, "108");
        var otherService = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Contract, "109");
        db.LegalServices.AddRange(requestService, otherService);
        await db.SaveChangesAsync();

        db.LawyerServices.Add(TestEntityFactory.CreateLawyerService(lawyer.Id, requestService.Id, 200m));
        db.LawyerServices.Add(TestEntityFactory.CreateLawyerService(lawyer.Id, otherService.Id, 300m));
        await db.SaveChangesAsync();

        var cairo = GetGovernorate( "Cairo");
        var nasrCity = GetCity( cairo.Id, "Nasr City");

        var request = new BroadcastRequest
        {
            RequestId = "REQ-OFFER-1",
            Title = "Broadcast",
            ProblemDetails = "details",
            GovernorateId = cairo.Id,
            Governorate = cairo.Name,
            CityId = nasrCity.Id,
            City = nasrCity.Name,
            Budget = 500m,
            ClientId = client.Id,
            LawyerServiceLegalServiceId = requestService.Id,
            Status = ClientRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        db.BroadcastRequests.Add(request);
        await db.SaveChangesAsync();

        var handler = new SendOfferCommandHandler(db, new FixedCurrentUserService(lawyer.Id));
        var result = await handler.Handle(
            new SendOfferCommand(request.Id, otherService.Id, 450m, "offer"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Offer.InvalidService", result.Error?.Code);
    }

    [Fact]
    public async Task GetLawyersByService_WithLocationFilter_ReturnsOnlyMatchingLawyers()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var admin = TestEntityFactory.CreateAdmin("109");
        var cairoLawyer = TestEntityFactory.CreateLawyer("109");
        cairoLawyer.City = "Nasr City";
        var gizaLawyer = TestEntityFactory.CreateLawyer("110");
        gizaLawyer.Governorate = "Giza";
        gizaLawyer.City = "Dokki";

        db.AddRange(admin, cairoLawyer, gizaLawyer);
        await db.SaveChangesAsync();

        var service = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Lawsuit, "110");
        db.LegalServices.Add(service);
        await db.SaveChangesAsync();

        db.LawyerServices.Add(TestEntityFactory.CreateLawyerService(cairoLawyer.Id, service.Id, 200m));
        db.LawyerServices.Add(TestEntityFactory.CreateLawyerService(gizaLawyer.Id, service.Id, 220m));
        await db.SaveChangesAsync();

        var giza = GetGovernorate("Giza");
        var dokki = GetCity(giza.Id, "Dokki");

        var handler = new GetLawyersByServiceQueryHandler(db, TestLocationCatalog.Instance);
        var result = await handler.Handle(
            new GetLawyersByServiceQuery(service.Id, giza.Id, dokki.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!);
        Assert.Equal(gizaLawyer.Id, dto.LawyerId);
    }

    [Fact]
    public async Task GetBroadcastRequests_FiltersByBudgetRange_AndReturnsPagedResult()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var admin = TestEntityFactory.CreateAdmin("111");
        var client = TestEntityFactory.CreateClient("111");
        db.AddRange(admin, client);
        await db.SaveChangesAsync();

        var service = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Consultation, "111");
        db.LegalServices.Add(service);
        await db.SaveChangesAsync();

        var cairo = GetGovernorate("Cairo");
        var nasrCity = GetCity(cairo.Id, "Nasr City");

        db.BroadcastRequests.AddRange(
            new BroadcastRequest
            {
                RequestId = "REQ-BUDGET-1",
                Title = "Low budget",
                ProblemDetails = "details",
                GovernorateId = cairo.Id,
                Governorate = cairo.Name,
                CityId = nasrCity.Id,
                City = nasrCity.Name,
                Budget = 300m,
                ClientId = client.Id,
            LawyerServiceLegalServiceId = service.Id,
            LegalServiceId = service.Id,
            ServiceType = service.ServiceType,
                Status = ClientRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            },
            new BroadcastRequest
            {
                RequestId = "REQ-BUDGET-2",
                Title = "High budget",
                ProblemDetails = "details",
                GovernorateId = cairo.Id,
                Governorate = cairo.Name,
                CityId = nasrCity.Id,
                City = nasrCity.Name,
                Budget = 900m,
                ClientId = client.Id,
                LawyerServiceLegalServiceId = service.Id,
                Status = ClientRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddMinutes(1)
            });
        await db.SaveChangesAsync();

        var handler = new GetBroadcastRequestsQueryHandler(db, TestLocationCatalog.Instance);
        var result = await handler.Handle(
            new GetBroadcastRequestsQuery(MinBudget: 500m, MaxBudget: 1000m, PageSize: 10),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.HasMore);
        Assert.Null(result.Value.NextCursor);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal("REQ-BUDGET-2", item.RequestId);
    }

    [Fact]
    public async Task GetBroadcastRequests_UsesCursorPagination_WithStableOrdering()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var admin = TestEntityFactory.CreateAdmin("1111");
        var client = TestEntityFactory.CreateClient("1111");
        db.AddRange(admin, client);
        await db.SaveChangesAsync();

        var service = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Consultation, "1111");
        db.LegalServices.Add(service);
        await db.SaveChangesAsync();

        var cairo = GetGovernorate("Cairo");
        var nasrCity = GetCity(cairo.Id, "Nasr City");

        db.BroadcastRequests.AddRange(
            new BroadcastRequest
            {
                RequestId = "REQ-CURSOR-1",
                Title = "Older request",
                ProblemDetails = "details",
                GovernorateId = cairo.Id,
                Governorate = cairo.Name,
                CityId = nasrCity.Id,
                City = nasrCity.Name,
                Budget = 300m,
                ClientId = client.Id,
                LawyerServiceLegalServiceId = service.Id,
                Status = ClientRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            },
            new BroadcastRequest
            {
                RequestId = "REQ-CURSOR-2",
                Title = "Newer request",
                ProblemDetails = "details",
                GovernorateId = cairo.Id,
                Governorate = cairo.Name,
                CityId = nasrCity.Id,
                City = nasrCity.Name,
                Budget = 900m,
                ClientId = client.Id,
                LawyerServiceLegalServiceId = service.Id,
                Status = ClientRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });
        await db.SaveChangesAsync();

        var handler = new GetBroadcastRequestsQueryHandler(db, TestLocationCatalog.Instance);

        var firstPage = await handler.Handle(
            new GetBroadcastRequestsQuery(PageSize: 1),
            CancellationToken.None);

        Assert.True(firstPage.IsSuccess);
        Assert.True(firstPage.Value!.HasMore);
        Assert.NotNull(firstPage.Value.NextCursor);
        Assert.Single(firstPage.Value.Items);
        Assert.Equal("REQ-CURSOR-2", firstPage.Value.Items[0].RequestId);

        var secondPage = await handler.Handle(
            new GetBroadcastRequestsQuery(Cursor: firstPage.Value.NextCursor, PageSize: 1),
            CancellationToken.None);

        Assert.True(secondPage.IsSuccess);
        Assert.False(secondPage.Value!.HasMore);
        Assert.Null(secondPage.Value.NextCursor);
        Assert.Single(secondPage.Value.Items);
        Assert.Equal("REQ-CURSOR-1", secondPage.Value.Items[0].RequestId);
    }

    [Fact]
    public async Task GetBroadcastRequests_DoesNotExposePendingRequestDocumentsToLawyers()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var client = TestEntityFactory.CreateClient("111-docs");
        db.Clients.Add(client);
        await db.SaveChangesAsync();
        var request = new BroadcastRequest
        {
            RequestId = "REQ-BROADCAST-PRIVATE-DOC",
            Title = "Private evidence",
            ProblemDetails = "details",
            Governorate = "Cairo",
            Budget = 1000m,
            ClientId = client.Id,
            Status = ClientRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        db.BroadcastRequests.Add(request);
        await db.SaveChangesAsync();
        db.CaseDocuments.Add(new CaseDocuments
        {
            DocumentName = "evidence.pdf",
            DocumentUrl = "documents/private-evidence",
            UploadedByUserId = client.Id,
            ClientRequestId = request.Id,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var result = await new GetBroadcastRequestsQueryHandler(db, TestLocationCatalog.Instance)
            .Handle(new GetBroadcastRequestsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(Assert.Single(result.Value!.Items).Documents);
    }

    [Fact]
    public async Task GetBroadcastRequests_FiltersByServiceType()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var admin = TestEntityFactory.CreateAdmin("112");
        var client = TestEntityFactory.CreateClient("112");
        db.AddRange(admin, client);
        await db.SaveChangesAsync();

        var consultationService = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Consultation, "112");
        var contractService = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Contract, "113");
        db.LegalServices.AddRange(consultationService, contractService);
        await db.SaveChangesAsync();

        var cairo = GetGovernorate("Cairo");
        var nasrCity = GetCity(cairo.Id, "Nasr City");

        db.BroadcastRequests.AddRange(
            new BroadcastRequest
            {
                RequestId = "REQ-TYPE-1",
                Title = "Consultation request",
                ProblemDetails = "details",
                GovernorateId = cairo.Id,
                Governorate = cairo.Name,
                CityId = nasrCity.Id,
                City = nasrCity.Name,
                Budget = 400m,
                ClientId = client.Id,
                LawyerServiceLegalServiceId = consultationService.Id,
                LegalServiceId = consultationService.Id,
                ServiceType = consultationService.ServiceType,
                Status = ClientRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            },
            new BroadcastRequest
            {
                RequestId = "REQ-TYPE-2",
                Title = "Contract request",
                ProblemDetails = "details",
                GovernorateId = cairo.Id,
                Governorate = cairo.Name,
                CityId = nasrCity.Id,
                City = nasrCity.Name,
                Budget = 600m,
                ClientId = client.Id,
                LawyerServiceLegalServiceId = contractService.Id,
                LegalServiceId = contractService.Id,
                ServiceType = contractService.ServiceType,
                Status = ClientRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddMinutes(1)
            });
        await db.SaveChangesAsync();

        var handler = new GetBroadcastRequestsQueryHandler(db, TestLocationCatalog.Instance);
        var result = await handler.Handle(
            new GetBroadcastRequestsQuery(ServiceType: ServiceType.Contract),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!.Items);
        Assert.Equal("REQ-TYPE-2", dto.RequestId);
    }

    [Fact]
    public async Task GetBroadcastRequests_FiltersByLocation_AndPaginates()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var admin = TestEntityFactory.CreateAdmin("113");
        var client = TestEntityFactory.CreateClient("113");
        db.AddRange(admin, client);
        await db.SaveChangesAsync();

        var service = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Consultation, "114");
        db.LegalServices.Add(service);
        await db.SaveChangesAsync();

        var cairo = GetGovernorate("Cairo");
        var nasrCity = GetCity(cairo.Id, "Nasr City");
        var giza = GetGovernorate("Giza");
        var dokki = GetCity(giza.Id, "Dokki");

        db.BroadcastRequests.AddRange(
            new BroadcastRequest
            {
                RequestId = "REQ-LOC-1",
                Title = "Cairo request",
                ProblemDetails = "details",
                GovernorateId = cairo.Id,
                Governorate = cairo.Name,
                CityId = nasrCity.Id,
                City = nasrCity.Name,
                Budget = 400m,
                ClientId = client.Id,
                LawyerServiceLegalServiceId = service.Id,
                Status = ClientRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            },
            new BroadcastRequest
            {
                RequestId = "REQ-LOC-2",
                Title = "Giza request",
                ProblemDetails = "details",
                GovernorateId = giza.Id,
                Governorate = giza.Name,
                CityId = dokki.Id,
                City = dokki.Name,
                Budget = 500m,
                ClientId = client.Id,
                LawyerServiceLegalServiceId = service.Id,
                Status = ClientRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddMinutes(1)
            });
        await db.SaveChangesAsync();

        var handler = new GetBroadcastRequestsQueryHandler(db, TestLocationCatalog.Instance);
        var result = await handler.Handle(
            new GetBroadcastRequestsQuery(
                GovernorateId: giza.Id,
                CityId: dokki.Id,
                PageSize: 1),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.HasMore);
        Assert.Null(result.Value.NextCursor);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal("REQ-LOC-2", item.RequestId);
    }

    private static (int Id, string Name) GetGovernorate(string englishName)
    {
        var governorate = TestLocationCatalog.GetGovernorateByEnglishName(englishName);
        return (governorate.Id, governorate.Name);
    }

    private static (int Id, string Name) GetCity(int governorateId, string englishName)
    {
        var city = TestLocationCatalog.GetCityByEnglishName(governorateId, englishName);
        return (city.Id, city.Name);
    }
}


