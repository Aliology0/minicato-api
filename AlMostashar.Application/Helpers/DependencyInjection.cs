using AlMostashar.Application.Common.Behaviors;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Escrows.Services;
using AlMostashar.Application.Features.ClientRequests.RequestDetails;
using AlMostashar.Application.Features.ClientRequests.Services;
using AlMostashar.Application.Features.Cases.Services;
using AlMostashar.Application.Features.Documents.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AlMostashar.Application.Helpers
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjection).Assembly;

            services.AddMediatR(option =>
            {
                option.RegisterServicesFromAssembly(assembly);
            });

            // Register all FluentValidation validators from this assembly
            services.AddValidatorsFromAssembly(assembly);

            // Register the validation pipeline behavior
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // Register the transaction pipeline behavior (runs after validation)
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

            // Escrow funding service — idempotent, callable from event handlers and recovery paths
            services.AddScoped<IEscrowFundingService, EscrowFundingService>();
            services.AddScoped<IEscrowSettlementService, EscrowSettlementService>();
            services.AddScoped<IRequestDetailsService, RequestDetailsService>();
            services.AddScoped<IClientRequestAttachmentService, ClientRequestAttachmentService>();
            services.AddScoped<ICaseMapper, CaseMapper>();
            services.AddScoped<IClientRequestCaseMapper, ClientRequestCaseMapper>();
            services.AddScoped<IDocumentAccessService, DocumentAccessService>();
            services.AddScoped<IUnlinkedDocumentCleanupService, UnlinkedDocumentCleanupService>();

            return services;
        }
    }
}
