using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.LawyerService.Commands.AddLawyerService;
using AlMostashar.Application.Features.LawyerService.Commands.UpdateLawyerService;
using AlMostashar.Application.Features.LawyerService.Queries.GetLawyerServices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers
{
    [Route("api/lawyer-services")]
    [Authorize(Roles ="Lawyer")]
    [ApiController]
    public class LawyerServiceController : ControllerBase
    {
        private readonly IMediator mediator;

        public LawyerServiceController(IMediator mediator)
        {
            this.mediator = mediator;
        }
        [HttpPost]
        public async Task<IActionResult> AddLawyerService(
            AddLawyerServiceCommand command,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetLawyerServices(CancellationToken cancellationToken)
        {
            var query = new GetLawyerServicesQuery();
            var result = await mediator.Send(query, cancellationToken);
            return this.ToActionResult(result);
        }

        [HttpPut("{serviceId}")]
        public async Task<IActionResult> UpdateLawyerService(
            [FromRoute] int serviceId,
            [FromBody] UpdateLawyerServiceCommand command,
            CancellationToken cancellationToken)
        {
            command.ServiceId = serviceId;
            var result = await mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }
    }
}
