using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.Reports.Commands.CreateReport;
using AlMostashar.Application.Features.Reports.Queries.GetReportDetails;
using AlMostashar.Application.Features.Reports.Queries.GetReports;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Submit a report against a lawyer, a user, or the platform itself.
    /// Accessible by both Client and Lawyer roles.
    /// </summary>
    /// <remarks>
    /// - Provide <c>lawyerId</c> to report a specific lawyer.
    /// - Provide <c>userId</c> to report any other user.
    /// - Leave both null to file a platform-level report.
    /// - <c>messages</c> are optional — decrypted locally from the device and sent as evidence.
    /// - <c>attachmentIds</c> are optional IDs returned by the authenticated document upload endpoint.
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "Client,Lawyer")]
    public async Task<IActionResult> CreateReport(
        [FromBody] CreateReportCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>
    /// Get a paginated list of all reports. Admin only.
    /// Supports filtering by status and cursor-based pagination.
    /// </summary>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetReportsAdmin(
        [FromQuery] GetReportsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>
    /// Get detailed information of a specific report, including deserialized evidence (messages and attachments).
    /// Admin only.
    /// </summary>
    [HttpGet("admin/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetReportDetailsAdmin(
        int id,
        CancellationToken cancellationToken)
    {
        var query = new GetReportDetailsQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>
    /// Update the status of a report (e.g. mark as Resolved or Dismissed).
    /// Admin only.
    /// </summary>
    [HttpPatch("admin/{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateReportStatusAdmin(
        int id,
        [FromBody] AlMostashar.Application.Features.Reports.Commands.UpdateReportStatus.UpdateReportStatusCommand command,
        CancellationToken cancellationToken)
    {
        command.ReportId = id;
        var result = await _mediator.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }
}

