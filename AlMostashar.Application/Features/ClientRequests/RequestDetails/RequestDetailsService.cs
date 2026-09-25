using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using FluentValidation;
using System.Text.Json;
using System.Text.Json.Serialization;
using AlMostashar.Domain.Entities;

namespace AlMostashar.Application.Features.ClientRequests.RequestDetails;

public sealed class RequestDetailsService : IRequestDetailsService
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public Result<ValidatedRequestDetails> ValidateAndNormalize(ServiceType serviceType, JsonElement requestDetails)
    {
        if (requestDetails.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            return Result<ValidatedRequestDetails>.Failure(new Error("RequestDetails.Required", "Request details are required."));

        try
        {
            object details = serviceType switch
            {
                ServiceType.Consultation => Deserialize<ConsultationRequestDetailsDto>(requestDetails),
                ServiceType.Contract => Deserialize<ContractRequestDetailsDto>(requestDetails),
                ServiceType.CompanyFormation => Deserialize<CompanyFormationRequestDetailsDto>(requestDetails),
                ServiceType.Lawsuit => Deserialize<LawsuitRequestDetailsDto>(requestDetails),
                _ => Deserialize<GenericRequestDetailsDto>(requestDetails)
            };

            var validation = Validate(details);
            if (!validation.IsValid)
            {
                var messages = validation.Errors.Select(e => e.ErrorMessage).Distinct().ToArray();
                return Result<ValidatedRequestDetails>.Failure(
                    new Error("RequestDetails.Invalid", string.Join(" ", messages), messages));
            }

            return Result<ValidatedRequestDetails>.Success(new ValidatedRequestDetails(details));
        }
        catch (JsonException ex)
        {
            return Result<ValidatedRequestDetails>.Failure(
                new Error("RequestDetails.InvalidJson", "Request details do not match the selected legal service.", ex.Message));
        }
    }

    public void AttachToRequest(ClientRequest request, object details)
    {
        switch (details)
        {
            case ConsultationRequestDetailsDto d: request.ConsultationDetails = new ConsultationRequestDetails { ClientRequest = request, LegalBranch = d.LegalBranch, PreferredAppointmentDate = d.PreferredAppointmentDate, CommunicationMethod = d.CommunicationMethod, ConsultationSummary = d.ConsultationSummary }; break;
            case ContractRequestDetailsDto d: request.ContractDetails = new ContractRequestDetails { ClientRequest = request, ContractType = d.ContractType, ContractRequestType = d.ContractRequestType, Language = d.Language, PagesCount = d.PagesCount, AllowedRevisions = d.AllowedRevisions, DeliveryDate = d.DeliveryDate, OtherPartyName = d.OtherPartyName }; break;
            case LawsuitRequestDetailsDto d: request.LawsuitDetails = new LawsuitRequestDetails { ClientRequest = request, LegalBranch = d.LegalBranch, IsCaseAlreadyFiled = d.IsCaseAlreadyFiled, CourtName = d.CourtName, CaseNumber = d.CaseNumber, NextHearingDate = d.NextHearingDate, LawsuitStatus = d.LawsuitStatus, ClientRole = d.ClientRole, OpponentName = d.OpponentName }; break;
            case CompanyFormationRequestDetailsDto d: request.CompanyFormationDetails = new CompanyFormationRequestDetails { ClientRequest = request, CompanyType = d.CompanyType, BusinessActivity = d.BusinessActivity, CapitalAmount = d.CapitalAmount, FoundersCount = d.FoundersCount, HasPowerOfAttorney = d.HasPowerOfAttorney, ProposedCompanyName = d.ProposedCompanyName }; break;
            case GenericRequestDetailsDto d: request.GenericDetails = new GenericRequestDetails { ClientRequest = request, LegalBranch = d.LegalBranch, Summary = d.Summary, DesiredOutcome = d.DesiredOutcome, ImportantDates = d.ImportantDates }; break;
            default: throw new NotSupportedException(details.GetType().Name);
        }
    }

    public object? MapFromEntity(ClientRequest request) => request.ServiceType switch
    {
        ServiceType.Consultation when request.ConsultationDetails is { } d => new ConsultationRequestDetailsDto(d.LegalBranch, d.PreferredAppointmentDate, d.CommunicationMethod, d.ConsultationSummary),
        ServiceType.Contract when request.ContractDetails is { } d => new ContractRequestDetailsDto(d.ContractType, d.ContractRequestType, d.Language, d.PagesCount, d.AllowedRevisions, d.DeliveryDate, d.OtherPartyName),
        ServiceType.Lawsuit when request.LawsuitDetails is { } d => new LawsuitRequestDetailsDto(d.LegalBranch, d.IsCaseAlreadyFiled, d.CourtName, d.CaseNumber, d.NextHearingDate, d.LawsuitStatus, d.ClientRole, d.OpponentName),
        ServiceType.CompanyFormation when request.CompanyFormationDetails is { } d => new CompanyFormationRequestDetailsDto(d.CompanyType, d.BusinessActivity, d.CapitalAmount, d.FoundersCount, d.HasPowerOfAttorney, d.ProposedCompanyName),
        _ when request.GenericDetails is { } d => new GenericRequestDetailsDto(d.LegalBranch, d.Summary, d.DesiredOutcome, d.ImportantDates),
        _ => null
    };

    private static T Deserialize<T>(JsonElement json) where T : class
        => json.Deserialize<T>(Options) ?? throw new JsonException("Request details cannot be null.");

    private static FluentValidation.Results.ValidationResult Validate(object details) => details switch
    {
        ConsultationRequestDetailsDto value => new ConsultationRequestDetailsValidator().Validate(value),
        ContractRequestDetailsDto value => new ContractRequestDetailsValidator().Validate(value),
        CompanyFormationRequestDetailsDto value => new CompanyFormationRequestDetailsValidator().Validate(value),
        LawsuitRequestDetailsDto value => new LawsuitRequestDetailsValidator().Validate(value),
        GenericRequestDetailsDto value => new GenericRequestDetailsValidator().Validate(value),
        _ => throw new NotSupportedException(details.GetType().Name)
    };
}

internal sealed class ConsultationRequestDetailsValidator : AbstractValidator<ConsultationRequestDetailsDto>
{
    public ConsultationRequestDetailsValidator()
    {
        RuleFor(x => x.LegalBranch).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CommunicationMethod).IsInEnum();
        RuleFor(x => x.PreferredAppointmentDate).GreaterThanOrEqualTo(DateTime.UtcNow.Date).When(x => x.PreferredAppointmentDate.HasValue);
        RuleFor(x => x.ConsultationSummary).MaximumLength(2000);
    }
}

internal sealed class ContractRequestDetailsValidator : AbstractValidator<ContractRequestDetailsDto>
{
    public ContractRequestDetailsValidator()
    {
        RuleFor(x => x.ContractType).IsInEnum();
        RuleFor(x => x.ContractRequestType).IsInEnum();
        RuleFor(x => x.Language).NotEmpty().MaximumLength(50);
        RuleFor(x => x.PagesCount).GreaterThan(0).When(x => x.PagesCount.HasValue);
        RuleFor(x => x.AllowedRevisions).GreaterThanOrEqualTo(0).When(x => x.AllowedRevisions.HasValue);
        RuleFor(x => x.DeliveryDate).GreaterThanOrEqualTo(DateTime.UtcNow.Date).When(x => x.DeliveryDate.HasValue);
        RuleFor(x => x.OtherPartyName).MaximumLength(200);
    }
}

internal sealed class CompanyFormationRequestDetailsValidator : AbstractValidator<CompanyFormationRequestDetailsDto>
{
    public CompanyFormationRequestDetailsValidator()
    {
        RuleFor(x => x.CompanyType).IsInEnum();
        RuleFor(x => x.BusinessActivity).NotEmpty().MaximumLength(500);
        RuleFor(x => x.FoundersCount).GreaterThan(0);
        RuleFor(x => x.CapitalAmount).GreaterThanOrEqualTo(0).When(x => x.CapitalAmount.HasValue);
        RuleFor(x => x.ProposedCompanyName).MaximumLength(200);
    }
}

internal sealed class LawsuitRequestDetailsValidator : AbstractValidator<LawsuitRequestDetailsDto>
{
    public LawsuitRequestDetailsValidator()
    {
        RuleFor(x => x.LegalBranch).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NextHearingDate).GreaterThanOrEqualTo(DateTime.UtcNow.Date).When(x => x.NextHearingDate.HasValue);
        RuleFor(x => x.ClientRole).NotNull().IsInEnum().When(x => x.IsCaseAlreadyFiled);
        RuleFor(x => x.LawsuitStatus).NotNull().IsInEnum().When(x => x.IsCaseAlreadyFiled);
        RuleFor(x => x).Must(x => !x.IsCaseAlreadyFiled || !string.IsNullOrWhiteSpace(x.CourtName) || !string.IsNullOrWhiteSpace(x.CaseNumber))
            .WithMessage("Court name or case number is required for a filed lawsuit.");
        RuleFor(x => x.CourtName).MaximumLength(200);
        RuleFor(x => x.CaseNumber).MaximumLength(100);
        RuleFor(x => x.OpponentName).MaximumLength(200);
    }
}

internal sealed class GenericRequestDetailsValidator : AbstractValidator<GenericRequestDetailsDto>
{
    public GenericRequestDetailsValidator()
    {
        RuleFor(x => x.LegalBranch).MaximumLength(100);
        RuleFor(x => x.Summary).MaximumLength(2000);
        RuleFor(x => x.DesiredOutcome).MaximumLength(1000);
        RuleFor(x => x.ImportantDates).MaximumLength(1000);
    }
}
