using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class ClientRequestConfiguration : IEntityTypeConfiguration<ClientRequest>
{
    public void Configure(EntityTypeBuilder<ClientRequest> builder)
    {
        // ─── TPH Discriminator (ClientRequest ← BroadcastRequest, DirectRequest) ───
        builder.HasDiscriminator<string>("RequestType")
            .HasValue<ClientRequest>("ClientRequest")
            .HasValue<BroadcastRequest>("BroadcastRequest")
            .HasValue<DirectRequest>("DirectRequest");

        // ─── Table ───
        builder.ToTable("ClientRequests");

        // ─── Properties ───
        builder.Property(cr => cr.RequestId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(cr => cr.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(cr => cr.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(cr => cr.ProblemDetails)
            .HasMaxLength(4000);

        builder.Property(cr => cr.Governorate)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(cr => cr.City)
            .HasMaxLength(100);

        builder.Property(cr => cr.ServiceType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(cr => cr.PreferredCommunicationMethod)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(cr => cr.Urgency)
            .HasConversion<string>()
            .HasMaxLength(30);

        // ─── Indexes ───
        builder.HasIndex(cr => cr.ClientId);
        builder.HasIndex(cr => cr.LegalServiceId);
        builder.HasIndex(cr => cr.ServiceType);
        builder.HasIndex(cr => cr.Status);
        builder.HasIndex(cr => cr.CreatedAt);
        builder.HasIndex(cr => cr.GovernorateId);
        builder.HasIndex(cr => cr.CityId);
        builder.HasIndex(cr => cr.LawyerServiceLegalServiceId);
        builder.HasIndex(cr => new { cr.LawyerServiceLawyerId, cr.LawyerServiceLegalServiceId });
        builder.HasIndex("RequestType", nameof(ClientRequest.Status), nameof(ClientRequest.CreatedAt), nameof(ClientRequest.Id));

        // ─── Relationships ───

        // 1:1 — ClientRequest → Invoice
        builder.HasOne(cr => cr.Invoice)
            .WithOne(i => i.ClientRequest)
            .HasForeignKey<Invoice>(i => i.ClientRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1:1 — ClientRequest → Escrow
        builder.HasOne(cr => cr.Escrow)
            .WithOne(e => e.Request)
            .HasForeignKey<Escrow>(e => e.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1:1 — ClientRequest → Feedback
        builder.HasOne(cr => cr.Feedback)
            .WithOne(f => f.ClientRequest)
            .HasForeignKey<Feedback>(f => f.ClientRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // N:1 — ClientRequest → LawyerService (composite FK)
        builder.HasOne(cr => cr.LawyersServices)
            .WithMany(ls => ls.Requests)
            .HasForeignKey(cr => new { cr.LawyerServiceLawyerId, cr.LawyerServiceLegalServiceId })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cr => cr.RequestedLegalService)
            .WithMany()
            .HasForeignKey(cr => cr.LegalServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
