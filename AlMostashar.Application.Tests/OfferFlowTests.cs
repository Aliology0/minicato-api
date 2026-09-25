using AlMostashar.Application.Features.ClientRequests.Commands.AcceptOffer;
using AlMostashar.Application.Features.ClientRequests.Commands.AcceptRequest;
using AlMostashar.Application.Features.ClientRequests.EventHandlers;
using AlMostashar.Application.Features.ClientRequests.Commands.SendOffer;
using AlMostashar.Application.Features.ClientRequests.Queries.GetClientOffers;
using AlMostashar.Application.Tests.TestSupport;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Events;
using AlMostashar.Domain.ValueObject.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AlMostashar.Application.Common.Interfaces;
using Moq;
using AlMostashar.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Application.Tests;

public class OfferFlowTests
{
    [Fact]
    public async Task GetClientOffers_UsesCursorPagination_AndSupportsRequestFilter()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var admin = TestEntityFactory.CreateAdmin("70");
        var client = TestEntityFactory.CreateClient("70");
        var lawyer1 = TestEntityFactory.CreateLawyer("70");
        var lawyer2 = TestEntityFactory.CreateLawyer("71");

        db.AddRange(admin, client, lawyer1, lawyer2);
        await db.SaveChangesAsync();

        var service = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Consultation, "70");
        db.LegalServices.Add(service);
        await db.SaveChangesAsync();

        db.LawyerServices.AddRange(
            TestEntityFactory.CreateLawyerService(lawyer1.Id, service.Id, 200m),
            TestEntityFactory.CreateLawyerService(lawyer2.Id, service.Id, 225m));
        await db.SaveChangesAsync();

        var request1 = new BroadcastRequest
        {
            RequestId = "REQ-B-70-1",
            Title = "Broadcast Request 1",
            ProblemDetails = "details",
            Governorate = "Cairo",
            Budget = 800m,
            ClientId = client.Id,
            LawyerServiceLegalServiceId = service.Id,
            Status = ClientRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        var request2 = new BroadcastRequest
        {
            RequestId = "REQ-B-70-2",
            Title = "Broadcast Request 2",
            ProblemDetails = "details",
            Governorate = "Cairo",
            Budget = 900m,
            ClientId = client.Id,
            LawyerServiceLegalServiceId = service.Id,
            Status = ClientRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow.AddMinutes(-4)
        };

        db.BroadcastRequests.AddRange(request1, request2);
        await db.SaveChangesAsync();

        db.RequestOffers.AddRange(
            new RequestOffer
            {
                ClientRequestId = request1.Id,
                LawyerId = lawyer1.Id,
                LegalServiceId = service.Id,
                OfferedAmount = 700m,
                Status = OfferStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddMinutes(-3)
            },
            new RequestOffer
            {
                ClientRequestId = request2.Id,
                LawyerId = lawyer2.Id,
                LegalServiceId = service.Id,
                OfferedAmount = 710m,
                Status = OfferStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddMinutes(-2)
            },
            new RequestOffer
            {
                ClientRequestId = request1.Id,
                LawyerId = lawyer2.Id,
                LegalServiceId = service.Id,
                OfferedAmount = 720m,
                Status = OfferStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            });
        await db.SaveChangesAsync();

        var handler = new GetClientOffersQueryHandler(db, new FixedCurrentUserService(client.Id));

        var firstPage = await handler.Handle(new GetClientOffersQuery(PageSize: 1), CancellationToken.None);

        Assert.True(firstPage.IsSuccess);
        Assert.NotNull(firstPage.Value);
        Assert.Single(firstPage.Value.Items);
        Assert.True(firstPage.Value.HasMore);
        Assert.NotNull(firstPage.Value.NextCursor);

        var secondPage = await handler.Handle(
            new GetClientOffersQuery(Cursor: firstPage.Value.NextCursor, PageSize: 1),
            CancellationToken.None);

        Assert.True(secondPage.IsSuccess);
        Assert.NotNull(secondPage.Value);
        Assert.Single(secondPage.Value.Items);
        Assert.NotEqual(firstPage.Value.Items[0].OfferId, secondPage.Value.Items[0].OfferId);

        var filtered = await handler.Handle(
            new GetClientOffersQuery(RequestId: request1.Id, PageSize: 10),
            CancellationToken.None);

        Assert.True(filtered.IsSuccess);
        Assert.NotNull(filtered.Value);
        Assert.Equal(2, filtered.Value.Items.Count);
        Assert.All(filtered.Value.Items, offer => Assert.Equal(request1.Id, offer.RequestId));
    }

    [Fact]
    public async Task SendOffer_DirectRequest_AssignedLawyer_UsesFixedLegalService()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var admin = TestEntityFactory.CreateAdmin("1");
        var client = TestEntityFactory.CreateClient("1");
        var lawyer = TestEntityFactory.CreateLawyer("1");

        db.AddRange(admin, client, lawyer);
        await db.SaveChangesAsync();

        var service = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Consultation, "1");
        db.LegalServices.Add(service);
        await db.SaveChangesAsync();

        var lawyerService = TestEntityFactory.CreateLawyerService(lawyer.Id, service.Id, 220m);
        db.LawyerServices.Add(lawyerService);
        await db.SaveChangesAsync();

        var request = new DirectRequest
        {
            RequestId = "REQ-D-1",
            Title = "Direct Request",
            ProblemDetails = "details",
            Governorate = "Cairo",
            ClientId = client.Id,
            Status = ClientRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            LawyerServiceLawyerId = lawyer.Id,
            LawyerServiceLegalServiceId = service.Id
        };

        db.DirectRequests.Add(request);
        await db.SaveChangesAsync();

        var handler = new SendOfferCommandHandler(db, new FixedCurrentUserService(lawyer.Id));
        var result = await handler.Handle(
            new SendOfferCommand(request.Id, null, 300m, "counter offer"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var savedOffer = await db.RequestOffers.SingleAsync();
        Assert.Equal(service.Id, savedOffer.LegalServiceId);
        Assert.Equal(OfferStatus.Pending, savedOffer.Status);
    }

    [Fact]
    public async Task SendOffer_DirectRequest_NonAssignedLawyer_IsBlocked()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var admin = TestEntityFactory.CreateAdmin("2");
        var client = TestEntityFactory.CreateClient("2");
        var assignedLawyer = TestEntityFactory.CreateLawyer("2");
        var otherLawyer = TestEntityFactory.CreateLawyer("3");

        db.AddRange(admin, client, assignedLawyer, otherLawyer);
        await db.SaveChangesAsync();

        var service = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Consultation, "2");
        db.LegalServices.Add(service);
        await db.SaveChangesAsync();

        var lawyerService = TestEntityFactory.CreateLawyerService(assignedLawyer.Id, service.Id, 200m);
        db.LawyerServices.Add(lawyerService);
        await db.SaveChangesAsync();

        var request = new DirectRequest
        {
            RequestId = "REQ-D-2",
            Title = "Direct Request",
            ProblemDetails = "details",
            Governorate = "Cairo",
            ClientId = client.Id,
            Status = ClientRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            LawyerServiceLawyerId = assignedLawyer.Id,
            LawyerServiceLegalServiceId = service.Id
        };

        db.DirectRequests.Add(request);
        await db.SaveChangesAsync();

        var handler = new SendOfferCommandHandler(db, new FixedCurrentUserService(otherLawyer.Id));
        var result = await handler.Handle(
            new SendOfferCommand(request.Id, service.Id, 300m, "counter offer"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Offer.Unauthorized", result.Error?.Code);
    }

    [Fact]
    public async Task SendOffer_DuplicatePending_Blocked_ButAllowedAfterResolution()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var admin = TestEntityFactory.CreateAdmin("3");
        var client = TestEntityFactory.CreateClient("3");
        var lawyer = TestEntityFactory.CreateLawyer("4");
        db.AddRange(admin, client, lawyer);
        await db.SaveChangesAsync();

        var service = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Consultation, "3");
        db.LegalServices.Add(service);
        await db.SaveChangesAsync();

        var lawyerService = TestEntityFactory.CreateLawyerService(lawyer.Id, service.Id, 180m);
        db.LawyerServices.Add(lawyerService);
        await db.SaveChangesAsync();

        var request = new BroadcastRequest
        {
            RequestId = "REQ-B-1",
            Title = "Broadcast Request",
            ProblemDetails = "details",
            Governorate = "Cairo",
            Budget = 500m,
            ClientId = client.Id,
            LawyerServiceLegalServiceId = service.Id,
            Status = ClientRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        db.BroadcastRequests.Add(request);
        await db.SaveChangesAsync();

        var handler = new SendOfferCommandHandler(db, new FixedCurrentUserService(lawyer.Id));

        var first = await handler.Handle(
            new SendOfferCommand(request.Id, service.Id, 450m, "first"),
            CancellationToken.None);
        Assert.True(first.IsSuccess);

        var second = await handler.Handle(
            new SendOfferCommand(request.Id, service.Id, 430m, "second"),
            CancellationToken.None);
        Assert.False(second.IsSuccess);
        Assert.Equal("Offer.Conflict", second.Error?.Code);

        var pending = await db.RequestOffers.SingleAsync();
        pending.Status = OfferStatus.Rejected;
        pending.RespondedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var third = await handler.Handle(
            new SendOfferCommand(request.Id, service.Id, 420m, "third"),
            CancellationToken.None);
        Assert.True(third.IsSuccess);

        var offersCount = await db.RequestOffers.CountAsync();
        Assert.Equal(2, offersCount);
    }

    /*
    PSEUDOCODE / PLAN:
    - Keep behavior intact: set up a callback publisher that may queue notifications until dbContext is ready.
    - Create test DB scope with the publisher and assign dbContext once scope is created.
    - After dbContext is assigned, process any queued notifications.
    - The previous implementation used a tuple deconstruction with (null!, null) which causes a typing/assignment error.
    - Fix by declaring a strongly typed list variable `toProcess` and populate it inside the lock.
    - Proceed with the rest of the test unchanged: create entities, add offers, accept an offer and assert expected results.
    */

    [Fact]
    public async Task AcceptOffer_AcceptsOne_AndRejectsOtherPendingOffers()
    {
        AlMostashar.Infrastructure.Data.AlmostasharDbContext? dbContext = null;
        var pending = new List<(object Notification, CancellationToken Token)>();

        var notificationService = new Mock<INotificationService>().Object;
        var logger = new Mock<ILogger<RequestAcceptedEventHandler>>().Object;

        var publisher = new CallbackPublisher(async (notification, cancellationToken) =>
        {
            // if dbContext not ready yet, queue the notification for later
            if (dbContext == null)
            {
                lock (pending)
                {
                    pending.Add((notification, cancellationToken));
                }
                return;
            }

            if (notification is RequestAcceptedEvent requestAcceptedEvent)
            {
                await new RequestAcceptedEventHandler(dbContext, notificationService, logger)
                    .Handle(requestAcceptedEvent, cancellationToken);
            }
        });

        // create scope and assign dbContext
        await using var scope = await TestDbContextScope.CreateAsync(publisher);
        var db = scope.DbContext;
        dbContext = db;

        // process any pending notifications now that dbContext is ready
        List<(object Notification, CancellationToken Token)> toProcess;
        lock (pending)
        {
            toProcess = pending.ToList();
            pending.Clear();
        }

        foreach (var (notification, token) in toProcess)
        {
            if (notification is RequestAcceptedEvent requestAcceptedEvent)
            {
                await new RequestAcceptedEventHandler(dbContext, notificationService, logger)
                    .Handle(requestAcceptedEvent, token);
            }
        }

        var admin = TestEntityFactory.CreateAdmin("4");
        var client = TestEntityFactory.CreateClient("4");
        var lawyer1 = TestEntityFactory.CreateLawyer("5");
        var lawyer2 = TestEntityFactory.CreateLawyer("6");
        db.AddRange(admin, client, lawyer1, lawyer2);
        await db.SaveChangesAsync();

        var service = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Consultation, "4");
        db.LegalServices.Add(service);
        await db.SaveChangesAsync();

        db.LawyerServices.Add(TestEntityFactory.CreateLawyerService(lawyer1.Id, service.Id, 200m));
        db.LawyerServices.Add(TestEntityFactory.CreateLawyerService(lawyer2.Id, service.Id, 210m));
        await db.SaveChangesAsync();

        var request = new BroadcastRequest
        {
            RequestId = "REQ-B-2",
            Title = "Broadcast Request",
            ProblemDetails = "details",
            Governorate = "Cairo",
            Budget = 600m,
            ClientId = client.Id,
            LawyerServiceLegalServiceId = service.Id,
            Status = ClientRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        db.BroadcastRequests.Add(request);
        await db.SaveChangesAsync();

        var offer1 = new RequestOffer
        {
            ClientRequestId = request.Id,
            LawyerId = lawyer1.Id,
            LegalServiceId = service.Id,
            OfferedAmount = 500m,
            Status = OfferStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        var offer2 = new RequestOffer
        {
            ClientRequestId = request.Id,
            LawyerId = lawyer2.Id,
            LegalServiceId = service.Id,
            OfferedAmount = 510m,
            Status = OfferStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        db.RequestOffers.AddRange(offer1, offer2);
        await db.SaveChangesAsync();

        var handler = new AcceptOfferCommandHandler(db, new FixedCurrentUserService(client.Id));
        var result = await handler.Handle(new AcceptOfferCommand(offer1.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var refreshedOffer1 = await db.RequestOffers.FirstAsync(o => o.Id == offer1.Id);
        var refreshedOffer2 = await db.RequestOffers.FirstAsync(o => o.Id == offer2.Id);
        var refreshedRequest = await db.ClientRequests.FirstAsync(r => r.Id == request.Id);
        var invoice = await db.Invoices.SingleAsync(i => i.ClientRequestId == request.Id);

        Assert.Equal(OfferStatus.Accepted, refreshedOffer1.Status);
        Assert.Equal(OfferStatus.Rejected, refreshedOffer2.Status);
        Assert.Equal(ClientRequestStatus.Accepted, refreshedRequest.Status);
        Assert.Equal(lawyer1.Id, refreshedRequest.LawyerServiceLawyerId);
        Assert.Equal(service.Id, refreshedRequest.LawyerServiceLegalServiceId);
        Assert.Equal(request.Id, result.Value.ClientRequestId);
        Assert.Equal(invoice.Id, result.Value.InvoiceId);
        Assert.Equal("Offer accepted successfully.", result.Value.Message);
    }

    [Fact]
    public async Task AcceptOffer_WhenAnotherOfferAlreadyAccepted_ReturnsConflict()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var admin = TestEntityFactory.CreateAdmin("41");
        var client = TestEntityFactory.CreateClient("41");
        var lawyer1 = TestEntityFactory.CreateLawyer("41");
        var lawyer2 = TestEntityFactory.CreateLawyer("42");
        db.AddRange(admin, client, lawyer1, lawyer2);
        await db.SaveChangesAsync();

        var service = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Consultation, "41");
        db.LegalServices.Add(service);
        await db.SaveChangesAsync();

        db.LawyerServices.Add(TestEntityFactory.CreateLawyerService(lawyer1.Id, service.Id, 240m));
        db.LawyerServices.Add(TestEntityFactory.CreateLawyerService(lawyer2.Id, service.Id, 245m));
        await db.SaveChangesAsync();

        var request = new BroadcastRequest
        {
            RequestId = "REQ-B-AC-1",
            Title = "Broadcast Request",
            ProblemDetails = "details",
            Governorate = "Cairo",
            Budget = 800m,
            ClientId = client.Id,
            LawyerServiceLegalServiceId = service.Id,
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
                OfferedAmount = 700m,
                Status = OfferStatus.Accepted,
                CreatedAt = DateTime.UtcNow,
                RespondedAt = DateTime.UtcNow
            },
            new RequestOffer
            {
                ClientRequestId = request.Id,
                LawyerId = lawyer2.Id,
                LegalServiceId = service.Id,
                OfferedAmount = 690m,
                Status = OfferStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });
        await db.SaveChangesAsync();

        var pendingOffer = await db.RequestOffers.FirstAsync(o => o.Status == OfferStatus.Pending);
        var handler = new AcceptOfferCommandHandler(db, new FixedCurrentUserService(client.Id));
        var result = await handler.Handle(new AcceptOfferCommand(pendingOffer.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Offer.Conflict", result.Error?.Code);
    }

    [Fact]
    public async Task RequestOffers_DbInvariant_AllowsOnlyOneAcceptedOfferPerRequest()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var admin = TestEntityFactory.CreateAdmin("51");
        var client = TestEntityFactory.CreateClient("51");
        var lawyer1 = TestEntityFactory.CreateLawyer("51");
        var lawyer2 = TestEntityFactory.CreateLawyer("52");
        db.AddRange(admin, client, lawyer1, lawyer2);
        await db.SaveChangesAsync();

        var service = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Consultation, "51");
        db.LegalServices.Add(service);
        await db.SaveChangesAsync();

        var request = new BroadcastRequest
        {
            RequestId = "REQ-B-UNQ-1",
            Title = "Broadcast Request",
            ProblemDetails = "details",
            Governorate = "Cairo",
            Budget = 900m,
            ClientId = client.Id,
            LawyerServiceLegalServiceId = service.Id,
            Status = ClientRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        db.BroadcastRequests.Add(request);
        await db.SaveChangesAsync();

        db.RequestOffers.Add(new RequestOffer
        {
            ClientRequestId = request.Id,
            LawyerId = lawyer1.Id,
            LegalServiceId = service.Id,
            OfferedAmount = 780m,
            Status = OfferStatus.Accepted,
            CreatedAt = DateTime.UtcNow,
            RespondedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        db.RequestOffers.Add(new RequestOffer
        {
            ClientRequestId = request.Id,
            LawyerId = lawyer2.Id,
            LegalServiceId = service.Id,
            OfferedAmount = 770m,
            Status = OfferStatus.Accepted,
            CreatedAt = DateTime.UtcNow,
            RespondedAt = DateTime.UtcNow
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task AcceptRequest_DirectRequest_WithMismatchedLegalService_IsRejected()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var admin = TestEntityFactory.CreateAdmin("5");
        var client = TestEntityFactory.CreateClient("5");
        var lawyer = TestEntityFactory.CreateLawyer("7");
        db.AddRange(admin, client, lawyer);
        await db.SaveChangesAsync();

        var service = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Consultation, "5");
        db.LegalServices.Add(service);
        await db.SaveChangesAsync();

        db.LawyerServices.Add(TestEntityFactory.CreateLawyerService(lawyer.Id, service.Id, 250m));
        await db.SaveChangesAsync();

        var request = new DirectRequest
        {
            RequestId = "REQ-D-3",
            Title = "Direct Request",
            ProblemDetails = "details",
            Governorate = "Cairo",
            ClientId = client.Id,
            Status = ClientRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            LawyerServiceLawyerId = lawyer.Id,
            LawyerServiceLegalServiceId = service.Id
        };

        db.DirectRequests.Add(request);
        await db.SaveChangesAsync();

        var handler = new AcceptRequestCommandHandler(db, new FixedCurrentUserService(lawyer.Id));
        var result = await handler.Handle(new AcceptRequestCommand(request.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Request.InvalidService", result.Error?.Code);
    }

    [Fact]
    public async Task AcceptRequest_BroadcastRequest_IsBlocked_MustUseOffers()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var admin = TestEntityFactory.CreateAdmin("60");
        var client = TestEntityFactory.CreateClient("60");
        var lawyer = TestEntityFactory.CreateLawyer("60");
        db.AddRange(admin, client, lawyer);
        await db.SaveChangesAsync();

        var service = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Consultation, "60");
        db.LegalServices.Add(service);
        await db.SaveChangesAsync();

        db.LawyerServices.Add(TestEntityFactory.CreateLawyerService(lawyer.Id, service.Id, 300m));
        await db.SaveChangesAsync();

        var request = new BroadcastRequest
        {
            RequestId = "REQ-B-60",
            Title = "Broadcast Request",
            ProblemDetails = "details",
            Governorate = "Cairo",
            Budget = 700m,
            ClientId = client.Id,
            LawyerServiceLegalServiceId = service.Id,
            Status = ClientRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        db.BroadcastRequests.Add(request);
        await db.SaveChangesAsync();

        var handler = new AcceptRequestCommandHandler(db, new FixedCurrentUserService(lawyer.Id));
        var result = await handler.Handle(new AcceptRequestCommand(request.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Request.BroadcastMustUseOffers", result.Error?.Code);
        var refreshed = await db.ClientRequests.FirstAsync(r => r.Id == request.Id);
        Assert.Equal(ClientRequestStatus.Pending, refreshed.Status);
    }
}
