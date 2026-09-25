using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class RequestOfferConfiguration : IEntityTypeConfiguration<RequestOffer>
{
    public void Configure(EntityTypeBuilder<RequestOffer> builder)
    {
        builder.ToTable("RequestOffers");

        builder.Property(o => o.OfferedAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.Note)
            .HasMaxLength(2000);

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Prevent multiple active offers by the same lawyer on the same request,
        // while still keeping historical offers (accepted/rejected/withdrawn).
        builder.HasIndex(o => new { o.ClientRequestId, o.LawyerId })
            .HasDatabaseName("UX_RequestOffers_PendingByRequestLawyer")
            .IsUnique()
            .HasFilter("[Status] = 'Pending'");

        // Allow exactly one accepted offer per request at the database level.
        builder.HasIndex(o => o.ClientRequestId)
            .HasDatabaseName("UX_RequestOffers_AcceptedPerRequest")
            .IsUnique()
            .HasFilter("[Status] = 'Accepted'");

        builder.HasIndex(o => o.Status);

        builder.HasOne(o => o.ClientRequest)
            .WithMany(r => r.Offers)
            .HasForeignKey(o => o.ClientRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Lawyer)
            .WithMany(l => l.Offers)
            .HasForeignKey(o => o.LawyerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.LegalService)
            .WithMany(s => s.RequestOffers)
            .HasForeignKey(o => o.LegalServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
