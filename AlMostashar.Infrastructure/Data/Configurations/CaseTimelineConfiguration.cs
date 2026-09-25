using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class CaseTimelineConfiguration : IEntityTypeConfiguration<CaseTimeline>
{
    public void Configure(EntityTypeBuilder<CaseTimeline> builder)
    {
        // ─── Table ───
        builder.ToTable("CaseTimelines");

        // ─── Properties ───
        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(t => t.Content)
            .HasMaxLength(4000);

        // ─── Indexes ───
        builder.HasIndex(t => t.CaseId);
    }
}
