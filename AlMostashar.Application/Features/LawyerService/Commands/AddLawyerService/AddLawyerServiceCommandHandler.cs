using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.LawyerService.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.LawyerService.Commands.AddLawyerService
{
    public class AddLawyerServiceCommandHandler : IRequestHandler<AddLawyerServiceCommand, Result<LawyerServiceResponse>>
    {
        private readonly IAppDbContext _dbContext;
        private readonly ICurrentUserService currentUserService;

        public AddLawyerServiceCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
        {
            _dbContext = context;
            this.currentUserService = currentUserService;
        }
        public async Task<Result<LawyerServiceResponse>> Handle(AddLawyerServiceCommand request, CancellationToken cancellationToken)
        {
            // Get Lawyer Id from the JWT
            var lawyerId = currentUserService.UserId;

            // Check if its valid and verified lawyer
            if (!await _dbContext.Lawyers.AnyAsync(l => l.Id == lawyerId && l.AccountStatus == AlMostashar.Domain.ValueObject.Enum.AccountStatus.Active, cancellationToken))
                return Result<LawyerServiceResponse>.Failure(
                    new Error("Lawyer.NotVerified", Messages.Generic.NotVerified(Messages.Fields.Lawyer)));

            // Get the service to validate the Id and to use Name and Summary
            var service = await _dbContext.LegalServices.AsNoTracking().FirstOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken);
            if (service == null)
                return Result<LawyerServiceResponse>.Failure(
                    new Error("Service.NotFound", Messages.Generic.NotFound(Messages.Fields.Service)));

            // Check if the lawyer has already added this service
            if (await _dbContext.LawyerServices.AnyAsync(ls => ls.LawyerId == lawyerId && ls.LegalServiceId == request.ServiceId, cancellationToken))
                return Result<LawyerServiceResponse>.Failure(
                    new Error("LawyerService.AlreadyExists", Messages.Generic.AlreadyExists(Messages.Fields.Service)));

            (var entity, var error) = Domain.Entities.LawyerService.Create(lawyerId, request.ServiceId, request.Price, request.Duration);

            if (error != null)
                return Result<LawyerServiceResponse>.Failure(error);

            await _dbContext.LawyerServices.AddAsync(entity!, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            var dto = new LawyerServiceResponse(entity!.LegalServiceId, service!.Title, service.Summary, entity.Price, entity.Duration, entity.IsActive);
            return Result<LawyerServiceResponse>.Success(dto);
        }
    }
}
