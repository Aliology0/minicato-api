using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.LegalService.DTOs;
using AlMostashar.Domain.Events;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.LegalService.Commands.AddLegalService
{
    public class AddLegalServiceCommandHandler
        : IRequestHandler<AddLegalServiceCommand, Result<LegalServiceResponse>>
    {
        private readonly IAppDbContext _dbContext;
        private readonly ICurrentUserService _currentUser;
        private readonly IPublisher _publisher;

        public AddLegalServiceCommandHandler(IAppDbContext dbContext, ICurrentUserService currentUser, IPublisher publisher)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
            _publisher = publisher;
        }

        public async Task<Result<LegalServiceResponse>> Handle(
            AddLegalServiceCommand request,
            CancellationToken cancellationToken)
        {
            var serviceType = (ServiceType)request.ServiceType;

            // Check if a LegalService with this ServiceType already exists
            if (await _dbContext.LegalServices.AnyAsync(
                    s => s.ServiceType == serviceType, cancellationToken))
            {
                return Result<LegalServiceResponse>.Failure(
                    new Error("LegalService.AlreadyExists",
                        Messages.Generic.AlreadyExists(Messages.Fields.ServiceType)));
            }

            // AdminId from JWT — not from the request body
            var adminId = _currentUser.UserId;

            // Create via domain factory (validates + raises domain event)
            var (entity, error) = Domain.Entities.LegalService.Create(
                request.Title,
                request.Summary,
                request.FullDescription,
                serviceType,
                adminId,
                request.IconUrl,
                request.RequiredDocuments,
                request.ExpectedDuration);

            if (error is not null || entity is  null)
                return Result<LegalServiceResponse>.Failure(error?? new Error("LegalService.NotFound", Messages.Generic.NotFound(Messages.Fields.Service)));

            await _dbContext.LegalServices.AddAsync(entity!, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _publisher.Publish(new LegalServiceCreatedEvent(entity.Id, entity.Title, entity.ServiceType, entity.AdminId));
            var dto = new LegalServiceResponse(
                entity.Id,
                entity.Title,
                entity.Summary,
                entity.FullDescription,
                entity.ServiceType.ToString(),
                entity.IconUrl,
                entity.RequiredDocuments,
                entity.ExpectedDuration,
                entity.IsActive,
                entity.CreatedAt);

            return Result<LegalServiceResponse>.Success(dto);
        }
    }
}
