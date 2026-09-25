using AlMostashar.Application.Features.ClientRequests.Services;
using AlMostashar.Application.Features.Documents.Commands.DeleteDocument;
using AlMostashar.Application.Features.Documents.Commands.GetPresignedUrl;
using AlMostashar.Application.Features.Documents.Commands.UploadDocument;
using AlMostashar.Application.Features.Documents.Services;
using AlMostashar.Application.Tests.TestSupport;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using AlMostashar.Application.Features.ClientRequests.DTOs;
using AlMostashar.Application.Features.Cases.DTOs;
using AlMostashar.Application.Features.Documents.DTOs;
using AlMostashar.Application.Features.Reports.Commands.CreateReport;
using AlMostashar.Application.Features.Reports.Queries.GetReportDetails;

namespace AlMostashar.Application.Tests;

public class DocumentSecurityFlowTests
{
    [Fact]
    public async Task Cleanup_ClaimsDocumentBeforeStorageDelete_AndClaimBlocksLinking()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var client = TestEntityFactory.CreateClient("411"); scope.DbContext.Clients.Add(client); await scope.DbContext.SaveChangesAsync();
        var document = CreateDocument(client.Id, "documents/stale"); document.CreatedAt = DateTime.UtcNow.AddDays(-3);
        scope.DbContext.CaseDocuments.Add(document); await scope.DbContext.SaveChangesAsync();
        var storage = new RecordingStorageService();
        storage.OnDelete = _ => Assert.False(string.IsNullOrWhiteSpace(scope.DbContext.CaseDocuments.AsNoTracking().Single().CleanupClaimToken));
        var cleanup = new UnlinkedDocumentCleanupService(scope.DbContext, storage);
        var count = await cleanup.CleanupAsync(DateTime.UtcNow.AddHours(-48), CancellationToken.None);
        Assert.Equal(1, count); Assert.False(await scope.DbContext.CaseDocuments.AnyAsync());

        var claimed = CreateDocument(client.Id, "documents/claimed"); claimed.CleanupClaimToken = Guid.NewGuid().ToString("D");
        scope.DbContext.CaseDocuments.Add(claimed); await scope.DbContext.SaveChangesAsync();
        var link = await new ClientRequestAttachmentService(scope.DbContext).LinkAsync(CreateRequest(client.Id, "REQ-CLAIM"), [claimed.Id], client.Id, CancellationToken.None);
        Assert.False(link.IsSuccess);
    }
    [Fact]
    public void PublicDocumentDtos_DoNotExposeStorageKeys()
    {
        Assert.Null(typeof(DocumentDto).GetProperty("DocumentUrl"));
        Assert.Null(typeof(CaseDocumentDto).GetProperty("DocumentUrl"));
        Assert.Null(typeof(UploadDocumentResponseDto).GetProperty("FileUrl"));
        Assert.Null(typeof(CreateReportCommand).GetProperty("AttachmentUrls"));
        Assert.NotNull(typeof(CreateReportCommand).GetProperty("AttachmentIds"));
        Assert.Null(typeof(ReportDetailsDto).GetProperty("AttachmentUrls"));
    }

    [Fact]
    public async Task PresignedUrl_AllowsTargetLawyerBeforeCaseCreation()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var client = TestEntityFactory.CreateClient("410"); var lawyer = TestEntityFactory.CreateLawyer("410");
        scope.DbContext.AddRange(client, lawyer); await scope.DbContext.SaveChangesAsync();
        var request = CreateRequest(client.Id, "REQ-TARGET-LAWYER"); 
        scope.DbContext.DirectRequests.Add(request); await scope.DbContext.SaveChangesAsync();
        var document = CreateDocument(client.Id, "documents/direct"); document.ClientRequestId = request.Id;
        scope.DbContext.CaseDocuments.Add(document); await scope.DbContext.SaveChangesAsync();
        var storage = new RecordingStorageService();
        var handler = new GetPresignedUrlCommandHandler(storage, new DocumentAccessService(scope.DbContext, new FixedCurrentUserService(lawyer.Id)));
        var result = await handler.Handle(new GetPresignedUrlCommand { DocumentId = document.Id }, CancellationToken.None);
        Assert.True(result.IsSuccess); Assert.Equal(document.DocumentUrl, storage.LastPresignedKey);
    }

    [Fact]
    public async Task BroadcastDocument_AllowsClientAndAcceptedLawyer_ButRejectsUnacceptedLawyer()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var client = TestEntityFactory.CreateClient("412");
        var acceptedLawyer = TestEntityFactory.CreateLawyer("413");
        var otherLawyer = TestEntityFactory.CreateLawyer("414");
        scope.DbContext.AddRange(client, acceptedLawyer, otherLawyer);
        await scope.DbContext.SaveChangesAsync();
        var request = new BroadcastRequest
        {
            RequestId = "REQ-BROADCAST-AUTH",
            Title = "Broadcast",
            ProblemDetails = "Details",
            Governorate = "Cairo",
            Budget = 5000m,
            ClientId = client.Id,
            LawyerServiceLawyerId = acceptedLawyer.Id,
            Status = ClientRequestStatus.Accepted,
            CreatedAt = DateTime.UtcNow
        };
        scope.DbContext.BroadcastRequests.Add(request);
        await scope.DbContext.SaveChangesAsync();
        var document = CreateDocument(client.Id, "documents/broadcast");
        document.ClientRequestId = request.Id;
        scope.DbContext.CaseDocuments.Add(document);
        await scope.DbContext.SaveChangesAsync();

        var clientResult = await new DocumentAccessService(scope.DbContext, new FixedCurrentUserService(client.Id))
            .GetForReadAsync(document.Id, CancellationToken.None);
        var acceptedResult = await new DocumentAccessService(scope.DbContext, new FixedCurrentUserService(acceptedLawyer.Id))
            .GetForReadAsync(document.Id, CancellationToken.None);
        var otherResult = await new DocumentAccessService(scope.DbContext, new FixedCurrentUserService(otherLawyer.Id))
            .GetForReadAsync(document.Id, CancellationToken.None);

        Assert.True(clientResult.IsSuccess);
        Assert.True(acceptedResult.IsSuccess);
        Assert.Equal("Document.Forbidden", otherResult.Error?.Code);
    }
    [Fact]
    public async Task UploadDocument_CreatesOwnedUnlinkedRecord_AndReturnsDocumentId()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var client = TestEntityFactory.CreateClient("401");
        scope.DbContext.Clients.Add(client);
        await scope.DbContext.SaveChangesAsync();
        var storage = new RecordingStorageService();
        await using var stream = new MemoryStream([1, 2, 3]);
        var file = new FormFile(stream, 0, stream.Length, "file", "rental-contract.pdf");
        var handler = new UploadDocumentCommandHandler(
            storage, scope.DbContext, new FixedCurrentUserService(client.Id));

        var result = await handler.Handle(
            new UploadDocumentCommand { File = file }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.DocumentId > 0);
        Assert.Equal("rental-contract.pdf", result.Value.DocumentName);
        var document = await scope.DbContext.CaseDocuments.SingleAsync();
        Assert.Equal(client.Id, document.UploadedByUserId);
        Assert.Null(document.ClientRequestId);
        Assert.Null(document.CaseId);
        Assert.Equal(storage.UploadedKey, document.DocumentUrl);
    }

    [Fact]
    public async Task AttachmentService_RejectsAnotherUsersDocumentAndAlreadyLinkedDocument()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var owner = TestEntityFactory.CreateClient("402");
        var other = TestEntityFactory.CreateClient("403");
        scope.DbContext.AddRange(owner, other);
        await scope.DbContext.SaveChangesAsync();
        var linkedRequest = CreateRequest(owner.Id, "REQ-LINKED");
        scope.DbContext.DirectRequests.Add(linkedRequest);
        await scope.DbContext.SaveChangesAsync();
        var otherDocument = CreateDocument(other.Id, "documents/other");
        var linkedDocument = CreateDocument(owner.Id, "documents/linked");
        linkedDocument.ClientRequestId = linkedRequest.Id;
        scope.DbContext.CaseDocuments.AddRange(otherDocument, linkedDocument);
        await scope.DbContext.SaveChangesAsync();
        var service = new ClientRequestAttachmentService(scope.DbContext);
        var target = CreateRequest(owner.Id, "REQ-TARGET");

        var otherResult = await service.LinkAsync(target, [otherDocument.Id], owner.Id, CancellationToken.None);
        var linkedResult = await service.LinkAsync(target, [linkedDocument.Id], owner.Id, CancellationToken.None);

        Assert.False(otherResult.IsSuccess);
        Assert.Equal("Request.InvalidAttachments", otherResult.Error?.Code);
        Assert.False(linkedResult.IsSuccess);
        Assert.Equal("Request.InvalidAttachments", linkedResult.Error?.Code);
    }

    [Fact]
    public async Task PresignedUrl_AllowsOwnerByDocumentId_AndNeverUsesCallerPath()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var client = TestEntityFactory.CreateClient("404");
        scope.DbContext.Clients.Add(client);
        await scope.DbContext.SaveChangesAsync();
        var document = CreateDocument(client.Id, "documents/owned-key");
        scope.DbContext.CaseDocuments.Add(document);
        await scope.DbContext.SaveChangesAsync();
        var storage = new RecordingStorageService();
        var handler = new GetPresignedUrlCommandHandler(
            storage,
            new DocumentAccessService(scope.DbContext, new FixedCurrentUserService(client.Id)));

        var result = await handler.Handle(
            new GetPresignedUrlCommand { DocumentId = document.Id, ExpirationMinutes = 15 },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(document.Id, result.Value!.DocumentId);
        Assert.Equal(document.DocumentUrl, storage.LastPresignedKey);
        Assert.Equal(15, storage.LastExpirationMinutes);
    }

    [Fact]
    public async Task PresignedUrl_RejectsUnauthorizedUserBeforeCallingStorage()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var owner = TestEntityFactory.CreateClient("405");
        var other = TestEntityFactory.CreateClient("406");
        scope.DbContext.AddRange(owner, other);
        await scope.DbContext.SaveChangesAsync();
        var document = CreateDocument(owner.Id, "documents/private-key");
        scope.DbContext.CaseDocuments.Add(document);
        await scope.DbContext.SaveChangesAsync();
        var storage = new RecordingStorageService();
        var handler = new GetPresignedUrlCommandHandler(
            storage,
            new DocumentAccessService(scope.DbContext, new FixedCurrentUserService(other.Id)));

        var result = await handler.Handle(
            new GetPresignedUrlCommand { DocumentId = document.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Document.Forbidden", result.Error?.Code);
        Assert.Null(storage.LastPresignedKey);
    }

    [Fact]
    public async Task Delete_AllowsOwnerForUnlinkedDocument_AndRemovesStorageAndRecord()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var owner = TestEntityFactory.CreateClient("407");
        scope.DbContext.Clients.Add(owner);
        await scope.DbContext.SaveChangesAsync();
        var document = CreateDocument(owner.Id, "documents/delete-me");
        scope.DbContext.CaseDocuments.Add(document);
        await scope.DbContext.SaveChangesAsync();
        var storage = new RecordingStorageService();
        var handler = new DeleteDocumentCommandHandler(
            storage,
            scope.DbContext,
            new DocumentAccessService(scope.DbContext, new FixedCurrentUserService(owner.Id)));

        var result = await handler.Handle(
            new DeleteDocumentCommand { DocumentId = document.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(document.DocumentUrl, storage.LastDeletedKey);
        Assert.False(await scope.DbContext.CaseDocuments.AnyAsync());
    }

    [Fact]
    public async Task Delete_RejectsAnotherUserAndLinkedDocumentsBeforeCallingStorage()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var owner = TestEntityFactory.CreateClient("408");
        var other = TestEntityFactory.CreateClient("409");
        scope.DbContext.AddRange(owner, other);
        await scope.DbContext.SaveChangesAsync();
        var request = CreateRequest(owner.Id, "REQ-DELETE-LINKED");
        scope.DbContext.DirectRequests.Add(request);
        await scope.DbContext.SaveChangesAsync();
        var unlinked = CreateDocument(owner.Id, "documents/not-yours");
        var linked = CreateDocument(owner.Id, "documents/linked-no-delete");
        linked.ClientRequestId = request.Id;
        scope.DbContext.CaseDocuments.AddRange(unlinked, linked);
        await scope.DbContext.SaveChangesAsync();
        var otherStorage = new RecordingStorageService();
        var otherHandler = new DeleteDocumentCommandHandler(
            otherStorage, scope.DbContext,
            new DocumentAccessService(scope.DbContext, new FixedCurrentUserService(other.Id)));
        var ownerStorage = new RecordingStorageService();
        var ownerHandler = new DeleteDocumentCommandHandler(
            ownerStorage, scope.DbContext,
            new DocumentAccessService(scope.DbContext, new FixedCurrentUserService(owner.Id)));

        var otherResult = await otherHandler.Handle(
            new DeleteDocumentCommand { DocumentId = unlinked.Id }, CancellationToken.None);
        var linkedResult = await ownerHandler.Handle(
            new DeleteDocumentCommand { DocumentId = linked.Id }, CancellationToken.None);

        Assert.Equal("Document.Forbidden", otherResult.Error?.Code);
        Assert.Null(otherStorage.LastDeletedKey);
        Assert.Equal("Document.Linked", linkedResult.Error?.Code);
        Assert.Null(ownerStorage.LastDeletedKey);
    }

    private static DirectRequest CreateRequest(int clientId, string requestId) => new()
    {
        RequestId = requestId,
        Title = "Request",
        ProblemDetails = "Details",
        Governorate = "Cairo",
        ClientId = clientId,
        Status = ClientRequestStatus.Pending,
        CreatedAt = DateTime.UtcNow
    };

    private static CaseDocuments CreateDocument(int ownerId, string key) => new()
    {
        DocumentName = "document.pdf",
        DocumentUrl = key,
        UploadedByUserId = ownerId,
        CreatedAt = DateTime.UtcNow
    };
}
