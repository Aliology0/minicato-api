using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class CaseNotesConfiguration : IEntityTypeConfiguration<CaseNotes>
{
    public void Configure(EntityTypeBuilder<CaseNotes> builder)
    {
        // ─── Table ───
        builder.ToTable("CaseNotes");

        // ─── Properties ───
        builder.Property(n => n.Content)
            .IsRequired()
            .HasMaxLength(4000);

        // ─── Indexes ───
        builder.HasIndex(n => n.CaseId);
    }
}
