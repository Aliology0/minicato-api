using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.LawyerService.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.LawyerService.Commands.UpdateLawyerService
{
    public class UpdateLawyerServiceCommandHandler : IRequestHandler<UpdateLawyerServiceCommand, Result<LawyerServiceResponse>>
    {
        private readonly IAppDbContext _dbContext;
        private readonly ICurrentUserService currentUserService;

        public UpdateLawyerServiceCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
        {
            _dbContext = context;
            this.currentUserService = currentUserService;
        }

        public async Task<Result<LawyerServiceResponse>> Handle(UpdateLawyerServiceCommand request, CancellationToken cancellationToken)
        {
            // Get Lawyer Id from the JWT
            var lawyerId = currentUserService.UserId;

            // Check if its valid and verified lawyer
            if (!await _dbContext.Lawyers.AnyAsync(l => l.Id == lawyerId && l.AccountStatus == AlMostashar.Domain.ValueObject.Enum.AccountStatus.Active, cancellationToken))
                return Result<LawyerServiceResponse>.Failure(
                    new Error("Lawyer.NotVerified", Messages.Generic.NotVerified(Messages.Fields.Lawyer)));

            // Get the service along with the legal service to use Name and Summary
            var entity = await _dbContext.LawyerServices
                .Include(ls => ls.LegalService)
                .FirstOrDefaultAsync(ls => ls.LawyerId == lawyerId && ls.LegalServiceId == request.ServiceId, cancellationToken);

            if (entity == null)
                return Result<LawyerServiceResponse>.Failure(
                    new Error("LawyerService.NotFound", Messages.Generic.NotFound(Messages.Fields.Service)));

            // Update properties if provided
            if (request.Price.HasValue)
            {
                var error = entity.UpdatePrice(request.Price.Value);
                if (error != null)
                    return Result<LawyerServiceResponse>.Failure(error);
            }

            if (request.Duration != null)
            {
                var error = entity.UpdateDuration(request.Duration);
                if (error != null)
                    return Result<LawyerServiceResponse>.Failure(error);
            }

            if (request.IsActive.HasValue)
            {
                if (request.IsActive.Value && !entity.IsActive)
                {
                    var error = entity.Activate();
                    if (error != null)
                        return Result<LawyerServiceResponse>.Failure(error);
                }
                else if (!request.IsActive.Value && entity.IsActive)
                {
                    var error = entity.Deactivate();
                    if (error != null)
                        return Result<LawyerServiceResponse>.Failure(error);
                }
            }

            _dbContext.LawyerServices.Update(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var dto = new LawyerServiceResponse(
                entity.LegalServiceId, 
                entity.LegalService.Title, 
                entity.LegalService.Summary, 
                entity.Price, 
                entity.Duration, 
                entity.IsActive);

            return Result<LawyerServiceResponse>.Success(dto);
        }
    }
}
