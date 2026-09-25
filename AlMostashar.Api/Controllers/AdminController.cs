using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.Admin.Commands.RegisterAdmin;
using AlMostashar.Application.Features.Admin.Commands.VerifyLawyer;
using AlMostashar.Application.Features.Admin.Queries.GetUsers;
using AlMostashar.Application.Features.Admin.Queries.UnVerifiedLawyers;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Register a new Admin account. Only allowed by Admins.</summary>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAdmin(
            [FromBody] RegisterAdminCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Verify (approve) a pending lawyer account.</summary>
        [HttpPut("lawyers/verify")]
        public async Task<IActionResult> VerifyLawyer(
            VerifyLawyerCommand verifyLawyerCommand,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(verifyLawyerCommand, cancellationToken);
            return this.ToActionResult(result);
        }

        [HttpGet("lawyers/unverified")]
        public async Task<IActionResult> GetUnVerifiedLawyers(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new UnVerifiedLawyersQuery(), cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>
        /// Get all users with optional filters and cursor-based pagination.
        /// For lawyers, IsVerified filter is also available.
        /// On the first page omit Cursor and CursorDate.
        /// For subsequent pages pass the NextCursor and NextCursorDate returned by the previous response.
        /// </summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] UserRole? role,
            [FromQuery] AccountStatus? accountStatus,
            [FromQuery] bool? isVerified,
            [FromQuery] string? search,
            [FromQuery] int? cursor,
            [FromQuery] DateTime? cursorDate,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetUsersQuery
            {
                Role = role,
                AccountStatus = accountStatus,
                IsVerified = isVerified,
                Search = search,
                Cursor = cursor,
                CursorDate = cursorDate,
                PageSize = pageSize
            }, cancellationToken);
            return this.ToActionResult(result);
        }
    }
}
