using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.LegalService.Commands.AddLegalService;
using AlMostashar.Application.Features.LegalService.Queries.GetLegalServices;
using AlMostashar.Application.Features.LegalService.Queries.GetServiceTypes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers
{
    [ApiController]
    [Route("api/legal-services")]
    [Authorize(Roles = "Admin")]
    public class LegalServiceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LegalServiceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Add a new legal service to the platform catalog.</summary>
        [HttpPost]
        public async Task<IActionResult> AddLegalService(
            AddLegalServiceCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Get all legal services.</summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllLegalServices(CancellationToken cancellationToken)
        {
            var query = new GetLegalServicesQuery();
            var result = await _mediator.Send(query, cancellationToken);
            return this.ToActionResult(result);
        }
        [HttpGet("types")]
        public async Task<IActionResult> GetServiceTypes(CancellationToken cancellationToken)
        {
            var query = new GetServiceTypesQuery();
            var result = await _mediator.Send(query, cancellationToken);
            return this.ToActionResult(result);
        }

    }
}
