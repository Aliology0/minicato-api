using AlMostashar.Application.Features.Cases.Commands.CreateCaseFromClientRequest;
using AlMostashar.Application.Features.Cases.Services;
using AlMostashar.Application.Features.ClientRequests.Commands.CreateBroadcastRequest;
using AlMostashar.Application.Features.ClientRequests.RequestDetails;
using AlMostashar.Application.Features.ClientRequests.Services;
using AlMostashar.Application.Tests.TestSupport;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AlMostashar.Application.Tests;

public class ClientRequestFlowRedesignTests
{
    private static readonly RequestDetailsService DetailsService = new();

    [Fact]
    public async Task TypedDetails_PersistConsultationAndCompanyFormationRows()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var client = TestEntityFactory.CreateClient("300");
        scope.DbContext.Clients.Add(client);
        await scope.DbContext.SaveChangesAsync();
        var consultation = new ClientRequest { RequestId = "REQ-TYPED-C", Title = "Consultation", ProblemDetails = "Details", Governorate = "Cairo", ClientId = client.Id, ServiceType = ServiceType.Consultation, Status = ClientRequestStatus.Pending, CreatedAt = DateTime.UtcNow };
        var company = new ClientRequest { RequestId = "REQ-TYPED-F", Title = "Company", ProblemDetails = "Details", Governorate = "Cairo", ClientId = client.Id, ServiceType = ServiceType.CompanyFormation, Status = ClientRequestStatus.Pending, CreatedAt = DateTime.UtcNow };
        DetailsService.AttachToRequest(consultation, new ConsultationRequestDetailsDto("Family", null, CommunicationMethod.Video, "نفقة"));
        DetailsService.AttachToRequest(company, new CompanyFormationRequestDetailsDto(CompanyType.LLC, "برمجيات", 100000, 2, true, "النيل"));
        scope.DbContext.ClientRequests.AddRange(consultation, company);
        await scope.DbContext.SaveChangesAsync();
        Assert.Equal("Family", (await scope.DbContext.ConsultationRequestDetails.SingleAsync()).LegalBranch);
        Assert.Equal("برمجيات", (await scope.DbContext.CompanyFormationRequestDetails.SingleAsync()).BusinessActivity);
    }

    [Fact]
    public async Task CreateBroadcastRequest_WithLawsuitDetails_ResolvesServiceTypeAndStoresNormalizedDetails()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var admin = TestEntityFactory.CreateAdmin("301");
        var client = TestEntityFactory.CreateClient("301");
        db.AddRange(admin, client);
        await db.SaveChangesAsync();
        var service = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Lawsuit, "301");
        db.LegalServices.Add(service);
        await db.SaveChangesAsync();

        var handler = new CreateBroadcastRequestCommandHandler(
            db, new FixedCurrentUserService(client.Id), TestLocationCatalog.Instance,
            DetailsService, new ClientRequestAttachmentService(db));
        var result = await handler.Handle(new CreateBroadcastRequestCommand
        {
            LegalServiceId = service.Id,
            Title = "دعوى عمالية",
            ProblemDetails = "تم إنهاء التعاقد دون صرف المستحقات.",
            GovernorateId = 1,
            CityId = 39,
            Budget = 5000m,
            Urgency = RequestUrgency.Urgent,
            RequestDetails = JsonSerializer.SerializeToElement(new
            {
                legalBranch = "Labor",
                isCaseAlreadyFiled = true,
                courtName = "محكمة شمال القاهرة الابتدائية",
                caseNumber = "123/2026",
                lawsuitStatus = "Active",
                clientRole = "Plaintiff"
            })
        }, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error?.Message);
        var saved = await db.BroadcastRequests.SingleAsync();
        Assert.Equal(service.Id, saved.LegalServiceId);
        Assert.Equal(ServiceType.Lawsuit, saved.ServiceType);
        Assert.Equal(RequestUrgency.Urgent, saved.Urgency);
        Assert.Equal("محكمة شمال القاهرة الابتدائية", saved.LawsuitDetails!.CourtName);
    }

    [Fact]
    public void InvalidContractDetails_FailStrongValidation()
    {
        var result = DetailsService.ValidateAndNormalize(
            ServiceType.Contract,
            JsonSerializer.SerializeToElement(new
            {
                contractType = "Rental",
                contractRequestType = "Review",
                language = "",
                pagesCount = 0
            }));

        Assert.False(result.IsSuccess);
        Assert.Equal("RequestDetails.Invalid", result.Error?.Code);
    }

    [Fact]
    public void LawyerServicePackage_EnforcesPricingAndUpdatesPackageFields()
    {
        var (invalid, _) = LawyerService.Create(1, 2, 300, "7 days");
        Assert.Null(invalid);

        var (package, error) = LawyerService.Create(
            1, 2, 2500m, "من 5 إلى 7 أيام");
        Assert.Null(error);
    }

    [Fact]
    public void ClientRequestCaseMapper_MapsAllDedicatedCaseTypes()
    {
        var mapper = new ClientRequestCaseMapper(new CaseMapper());
        var client = TestEntityFactory.CreateClient("302");
        var cases = new (ServiceType Type, object Details, Type Expected)[]
        {
            (ServiceType.Consultation, new ConsultationRequestDetailsDto("Family", null, CommunicationMethod.Video, "نفقة"), typeof(ConsultationCase)),
            (ServiceType.Contract, new ContractRequestDetailsDto(ContractType.Rental, ContractRequestType.Review, "Arabic", 5, 1, null, null), typeof(ContractCase)),
            (ServiceType.CompanyFormation, new CompanyFormationRequestDetailsDto(CompanyType.LLC, "برمجيات", 100000m, 2, true, "شركة النيل"), typeof(CompanyFormationCase)),
            (ServiceType.Lawsuit, new LawsuitRequestDetailsDto("Labor", true, "محكمة العمال", "44/2026", null, LawsuitStatus.Active, ClientRole.Plaintiff, "شركة"), typeof(LawsuitCase))
        };

        foreach (var item in cases)
        {
            var request = new DirectRequest
            {
                Title = "طلب قانوني",
                ProblemDetails = "تفاصيل",
                ServiceType = item.Type,
                Client = client
            };
            var result = mapper.Create(request, 10, item.Details);
            Assert.True(result.IsSuccess);
            Assert.IsType(item.Expected, result.Value);
            if (result.Value is ConsultationCase consultationCase)
                Assert.Null(consultationCase.AppointmentDate);
            if (result.Value is LawsuitCase lawsuitCase)
                Assert.Equal("Labor", lawsuitCase.LegalBranch);
        }
    }

    [Fact]
    public async Task CreateCaseFromClientRequest_IsIdempotentAndMapsContractDetails()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var admin = TestEntityFactory.CreateAdmin("303");
        var client = TestEntityFactory.CreateClient("303");
        var lawyer = TestEntityFactory.CreateLawyer("303");
        db.AddRange(admin, client, lawyer);
        await db.SaveChangesAsync();
        var service = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Contract, "303");
        db.LegalServices.Add(service);
        await db.SaveChangesAsync();
        db.LawyerServices.Add(TestEntityFactory.CreateLawyerService(lawyer.Id, service.Id, 2000m));
        await db.SaveChangesAsync();

        var details = DetailsService.ValidateAndNormalize(
            ServiceType.Contract,
            JsonSerializer.SerializeToElement(new
            {
                contractType = "Employment",
                contractRequestType = "Draft",
                language = "Arabic",
                allowedRevisions = 2
            })).Value!;

        var request = new DirectRequest
        {
            RequestId = "REQ-IDEMPOTENT",
            Title = "صياغة عقد عمل",
            ProblemDetails = "عقد متوافق مع قانون العمل المصري",
            Governorate = "القاهرة",
            ClientId = client.Id,
            Client = client,
            LegalServiceId = service.Id,
            ServiceType = ServiceType.Contract,
            LawyerServiceLawyerId = lawyer.Id,
            LawyerServiceLegalServiceId = service.Id,
            Status = ClientRequestStatus.InProgress,
            CreatedAt = DateTime.UtcNow
        };
        DetailsService.AttachToRequest(request, details.Details);
        db.DirectRequests.Add(request);
        await db.SaveChangesAsync();

        var handler = new CreateCaseFromClientRequestCommandHandler(
            db, DetailsService, new ClientRequestCaseMapper(new CaseMapper()), new NoOpPublisher());
        var first = await handler.Handle(new CreateCaseFromClientRequestCommand(request.Id), CancellationToken.None);
        var second = await handler.Handle(new CreateCaseFromClientRequestCommand(request.Id), CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(first.Value, second.Value);
        var created = Assert.IsType<ContractCase>(await db.Cases.SingleAsync());
        Assert.Equal(ContractType.Employment, created.ContractType);
        Assert.Equal(2, created.AllowedRevisions);
        Assert.Single(await db.CaseClientRequests.ToListAsync());
    }
}
