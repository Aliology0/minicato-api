using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        // ─── Table ───
        builder.ToTable("Feedbacks");

        // ─── Properties ───
        builder.Property(f => f.Content)
            .HasMaxLength(4000);

        // ─── Indexes ───
        builder.HasIndex(f => f.ClientRequestId)
            .IsUnique();
        
        // N:1 — ClientRequest → LawyerService (composite FK)
        builder.HasOne(fb => fb.LawyersServices)
            .WithMany(ls=>ls.Feedbacks)
            .HasForeignKey(cr => new { cr.LawyerServiceLawyerId, cr.LawyerServiceLegalServiceId })
            .OnDelete(DeleteBehavior.SetNull);

    }
}
