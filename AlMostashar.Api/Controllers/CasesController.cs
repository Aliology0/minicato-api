using AlMostashar.Application.Features.Cases.Commands.AddCaseNote;
using AlMostashar.Application.Features.Cases.Commands.AddCaseTimeline;
using AlMostashar.Application.Features.Cases.Commands.AddCase;
using AlMostashar.Application.Features.Cases.Commands.ConfirmCaseCompletion;
using AlMostashar.Application.Features.Cases.Commands.DeleteCaseDocument;
using AlMostashar.Application.Features.Cases.Commands.DeleteCaseNote;
using AlMostashar.Application.Features.Cases.Commands.StartCase;
using AlMostashar.Application.Features.Cases.Commands.CancelCase;
using AlMostashar.Application.Features.Cases.Commands.FinishCase;
using AlMostashar.Application.Features.Cases.Commands.UploadCaseDocument;
using AlMostashar.Application.Features.Cases.Queries.GetCaseById;
using AlMostashar.Application.Features.Cases.Queries.GetClientCases;
using AlMostashar.Application.Features.Cases.Queries.GetMyCases;
using AlMostashar.Domain.ValueObject.Enum;
using AlMostashar.Api.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers
{
    [ApiController]
    [Route("api/cases")]
    [Authorize]
    public class CasesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CasesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ─── Commands ─────────────────────────────────────────────────────────

        /// <summary>Lawyer manually creates a case (external/outside-platform case).</summary>
        [HttpPost]
        public async Task<IActionResult> CreateCase(
            [FromBody] CreateCaseRequest body,
            CancellationToken cancellationToken)
        {
            var command = new AddCaseCommand(
                Title:                body.Title,
                Description:          body.Description,
                ClientName:           body.ClientName,
                ServiceType:          body.ServiceType,
                AppointmentDate:      body.AppointmentDate,
                CommunicationMethod:  body.CommunicationMethod,
                LegalBranch:          body.LegalBranch,
                ContractType:         body.ContractType,
                Language:             body.Language,
                AllowedRevisions:     body.AllowedRevisions,
                DeliveryDate:         body.DeliveryDate,
                CompanyType:          body.CompanyType,
                CapitalAmount:        body.CapitalAmount,
                FoundersCount:        body.FoundersCount,
                HasPowerOfAttorney:   body.HasPowerOfAttorney,
                CourtName:            body.CourtName,
                CaseNumber:           body.CaseNumber,
                NextHearingDate:      body.NextHearingDate,
                LawsuitStatus:        body.LawsuitStatus,
                ClientRole:           body.ClientRole);

            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Lawyer starts a case (sets status to InProgress).</summary>
        [HttpPut("{caseId:int}/start")]
        public async Task<IActionResult> StartCase(
            int caseId,
            CancellationToken cancellationToken)
        {
            var command = new StartCaseCommand(CaseId: caseId);

            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Lawyer cancels a case (sets status to Canceled).</summary>
        [HttpPut("{caseId:int}/cancel")]
        public async Task<IActionResult> CancelCase(
            int caseId,
            [FromBody] CancelCaseRequest body,
            CancellationToken cancellationToken)
        {
            var command = new CancelCaseCommand(
                CaseId: caseId,
                Reason: body.Reason);

            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Lawyer finishes a case (sets status to PendingConfirmation).</summary>
        [HttpPut("{caseId:int}/finish")]
        public async Task<IActionResult> FinishCase(
            int caseId,
            CancellationToken cancellationToken)
        {
            var command = new FinishCaseCommand(CaseId: caseId);

            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Client confirms that a platform case service was completed.</summary>
        [HttpPost("{caseId:int}/confirm-completion")]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> ConfirmCompletion(
            int caseId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ConfirmCaseCompletionCommand(caseId), cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Lawyer adds a private note to a case.</summary>
        [HttpPost("{caseId:int}/notes")]
        public async Task<IActionResult> AddNote(
            int caseId,
            [FromBody] ContentRequest body,
            CancellationToken cancellationToken)
        {
            var command = new AddCaseNoteCommand(
                CaseId:   caseId,
                Content:  body.Content);

            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Lawyer deletes one of their case notes.</summary>
        [HttpDelete("{caseId:int}/notes/{noteId:int}")]
        public async Task<IActionResult> DeleteNote(
            int caseId, int noteId,
            CancellationToken cancellationToken)
        {
            var command = new DeleteCaseNoteCommand(
                CaseId:   caseId,
                NoteId:   noteId);

            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Lawyer adds a progress milestone to the case timeline.</summary>
        [HttpPost("{caseId:int}/timeline")]
        public async Task<IActionResult> AddTimeline(
            int caseId,
            [FromBody] TimelineRequest body,
            CancellationToken cancellationToken)
        {
            var command = new AddCaseTimelineCommand(
                CaseId:   caseId,
                Title:    body.Title,
                Content:  body.Content);

            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Lawyer or Client uploads a document to a case.</summary>
        [HttpPost("{caseId:int}/documents")]
        public async Task<IActionResult> UploadDocument(
            int caseId,
            [FromBody] DocumentRequest body,
            CancellationToken cancellationToken)
        {
            var command = new UploadCaseDocumentCommand(
                CaseId:       caseId,
                DocumentId:   body.DocumentId);

            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Lawyer deletes a document from a case.</summary>
        [HttpDelete("{caseId:int}/documents/{documentId:int}")]
        public async Task<IActionResult> DeleteDocument(
            int caseId, int documentId,
            CancellationToken cancellationToken)
        {
            var command = new DeleteCaseDocumentCommand(
                CaseId:     caseId,
                DocumentId: documentId);

            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        // ─── Queries ──────────────────────────────────────────────────────────

        /// <summary>Lawyer retrieves all their cases. Supports cursor pagination and optional filters.</summary>
        [HttpGet]
        public async Task<IActionResult> GetMyCases(
            [FromQuery] CaseStatus? status,
            [FromQuery] CaseSource? source,
            [FromQuery] ServiceType? serviceType,
            [FromQuery] string? search,
            [FromQuery] int? cursor,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new GetMyCasesQuery(
                Status: status,
                Source: source,
                ServiceType: serviceType,
                Search: search,
                Cursor: cursor,
                PageSize: pageSize);

            var result = await _mediator.Send(query, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Client retrieves all cases linked to them via their requests.</summary>
        [HttpGet("my-cases")]
        public async Task<IActionResult> GetClientCases(CancellationToken cancellationToken)
        {
            var query = new GetClientCasesQuery();
            var result = await _mediator.Send(query, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Get full details of a specific case (Lawyer or linked Client).</summary>
        [HttpGet("{caseId:int}")]
        public async Task<IActionResult> GetCaseById(
            int caseId,
            CancellationToken cancellationToken)
        {
            var query = new GetCaseByIdQuery(CaseId: caseId);
            var result = await _mediator.Send(query, cancellationToken);
            return this.ToActionResult(result);
        }
    }

    // ─── Request body models (thin — only what comes from HTTP body) ─────────

    public record CreateCaseRequest(
        string Title,
        string Description,
        string? ClientName,
        ServiceType ServiceType,
        DateTime? AppointmentDate,
        CommunicationMethod? CommunicationMethod,
        string? LegalBranch,
        ContractType? ContractType,
        string? Language,
        int? AllowedRevisions,
        DateTime? DeliveryDate,
        CompanyType? CompanyType,
        decimal? CapitalAmount,
        int? FoundersCount,
        bool? HasPowerOfAttorney,
        string? CourtName,
        string? CaseNumber,
        DateTime? NextHearingDate,
        LawsuitStatus? LawsuitStatus,
        ClientRole? ClientRole);

    public record CancelCaseRequest(string Reason);
    public record ContentRequest(string Content);
    public record TimelineRequest(string Title, string Content);
    public record DocumentRequest(int DocumentId);
}
