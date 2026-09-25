using AlMostashar.Application.Features.Cases.EventHandlers;
using AlMostashar.Application.Features.ClientRequests.Commands.PayInvoice;
using AlMostashar.Application.Tests.TestSupport;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Events;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AlMostashar.Application.Tests;

public class PayInvoiceAndEventsTests
{
    //[Fact]
    //public async Task PayInvoice_UpdatesState_AndPublishesInvoicePayedEvent()
    //{
    //    await using var scope = await TestDbContextScope.CreateAsync();
    //    var db = scope.DbContext;

    //    var client = await SeedAcceptedRequestWithInvoiceAsync(db, "20", "REQ-PAY-1");

    //    var mediator = new Mock<IMediator>();
    //    mediator
    //        .Setup(m => m.Publish(It.IsAny<InvoicePayedEvent>(), It.IsAny<CancellationToken>()))
    //        .Returns(Task.CompletedTask);

    //    var handler = new PayInvoiceCommandHandler(db, mediator.Object, new FixedCurrentUserService(client.Id));
    //    var invoiceId = await db.Invoices.Select(i => i.Id).SingleAsync();

    //    var result = await handler.Handle(new PayInvoiceCommand(invoiceId), CancellationToken.None);

    //    Assert.True(result.IsSuccess);

    //    var invoice = await db.Invoices.Include(i => i.ClientRequest).SingleAsync(i => i.Id == invoiceId);
    //    Assert.Equal(InvoiceStatus.Paid, invoice.Status);
    //    Assert.Equal(ClientRequestStatus.InProgress, invoice.ClientRequest.Status);

    //    mediator.Verify(
    //        m => m.Publish(It.IsAny<InvoicePayedEvent>(), It.IsAny<CancellationToken>()),
    //        Times.Once);
    //}

    //[Fact]
    //public async Task PayInvoice_WhenDownstreamPublishFails_RollsBackChanges()
    //{
    //    await using var scope = await TestDbContextScope.CreateAsync();
    //    var db = scope.DbContext;

    //    var client = await SeedAcceptedRequestWithInvoiceAsync(db, "21", "REQ-PAY-2");
    //    var invoiceId = await db.Invoices.Select(i => i.Id).SingleAsync();

    //    var mediator = new Mock<IMediator>();
    //    mediator
    //        .Setup(m => m.Publish(It.IsAny<InvoicePayedEvent>(), It.IsAny<CancellationToken>()))
    //        .ThrowsAsync(new InvalidOperationException("Downstream failure"));

    //    var handler = new PayInvoiceCommandHandler(db, mediator.Object, new FixedCurrentUserService(client.Id));

    //    await Assert.ThrowsAsync<InvalidOperationException>(() =>
    //        handler.Handle(new PayInvoiceCommand(invoiceId), CancellationToken.None));

    //    await using var verificationDb = scope.CreateNewContext();
    //    var invoice = await verificationDb.Invoices
    //        .Include(i => i.ClientRequest)
    //        .SingleAsync(i => i.Id == invoiceId);

    //    Assert.Equal(InvoiceStatus.Unpaid, invoice.Status);
    //    Assert.Equal(ClientRequestStatus.Accepted, invoice.ClientRequest.Status);
    //}

    //[Fact]
    //public async Task InvoicePayedEventHandler_IsIdempotent_DoesNotCreateDuplicateCase()
    //{
    //    await using var scope = await TestDbContextScope.CreateAsync();
    //    var db = scope.DbContext;

    //    var admin = TestEntityFactory.CreateAdmin("30");
    //    var client = TestEntityFactory.CreateClient("30");
    //    var lawyer = TestEntityFactory.CreateLawyer("30");
    //    db.AddRange(admin, client, lawyer);
    //    await db.SaveChangesAsync();

    //    var request = new DirectRequest
    //    {
    //        RequestId = "REQ-EVT-1",
    //        Title = "Request For Event",
    //        ProblemDetails = "details",
    //        Governorate = "Cairo",
    //        ClientId = client.Id,
    //        Status = ClientRequestStatus.Accepted,
    //        CreatedAt = DateTime.UtcNow,
    //        LawyerServiceLawyerId = lawyer.Id
    //    };
    //    db.DirectRequests.Add(request);
    //    await db.SaveChangesAsync();

    //    var publisher = new Mock<IPublisher>();
    //    var handler = new InvoicePayedEventHandler(db, publisher.Object);
    //    var evt = new InvoicePaidEvent(
    //        invoiceId: 1,
    //        clientRequestId: request.Id,
    //        lawyerId: lawyer.Id,
    //        title: request.Title,
    //        serviceType: ServiceType.Consultation,
    //        paymentId:1,
    //        transactionId: 12345,
    //        paymobOrderId: 98765);

    //    await handler.Handle(evt, CancellationToken.None);
    //    await handler.Handle(evt, CancellationToken.None);

    //    var linksCount = await db.CaseClientRequests.CountAsync(c => c.ClientRequestId == request.Id);
    //    Assert.Equal(1, linksCount);
    //}

    private static async Task<Client> SeedAcceptedRequestWithInvoiceAsync(
        AlMostashar.Infrastructure.Data.AlmostasharDbContext db,
        string suffix,
        string requestCode)
    {
        var admin = TestEntityFactory.CreateAdmin(suffix);
        var client = TestEntityFactory.CreateClient(suffix);
        var lawyer = TestEntityFactory.CreateLawyer(suffix);
        db.AddRange(admin, client, lawyer);
        await db.SaveChangesAsync();

        var service = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Consultation, suffix);
        db.LegalServices.Add(service);
        await db.SaveChangesAsync();

        db.LawyerServices.Add(TestEntityFactory.CreateLawyerService(lawyer.Id, service.Id, 350m));
        await db.SaveChangesAsync();

        var request = new DirectRequest
        {
            RequestId = requestCode,
            Title = "Payable Request",
            ProblemDetails = "details",
            Governorate = "Cairo",
            ClientId = client.Id,
            Status = ClientRequestStatus.Accepted,
            CreatedAt = DateTime.UtcNow,
            LawyerServiceLawyerId = lawyer.Id,
            LawyerServiceLegalServiceId = service.Id
        };
        db.DirectRequests.Add(request);
        await db.SaveChangesAsync();

        db.Invoices.Add(new Invoice
        {
            ReferenceNumber = $"INV-{suffix}",
            PlatformFee = 30m,
            LawyerAmount = 270m,
            TotalAmount = 300m,
            Status = InvoiceStatus.Unpaid,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(7),
            PaidAt = DateTime.MinValue,
            ClientRequestId = request.Id
        });
        await db.SaveChangesAsync();

        return client;
    }
}
