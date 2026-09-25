using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class DisputeConfiguration : IEntityTypeConfiguration<Dispute>
{
    public void Configure(EntityTypeBuilder<Dispute> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Reason)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(d => d.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(d => d.Priority)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(d => d.AdminDecision)
            .HasMaxLength(2000);

        builder.Property(d => d.AdminNotes)
            .HasMaxLength(4000);

        // ─── JSON Columns (Owned Types) ───
        builder.OwnsMany(d => d.Attachments, a =>
        {
            a.ToJson();
            a.Property(x => x.Name).HasMaxLength(500);
            a.Property(x => x.Path).HasMaxLength(2000);
        });

        builder.OwnsMany(d => d.DisputeChatMessages, m =>
        {
            m.ToJson();
            m.Property(x => x.Content).HasMaxLength(4000);
        });

        // ─── Relationships ───
        builder.HasOne(d => d.Case)
            .WithMany()
            .HasForeignKey(d => d.CaseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Escrow)
            .WithMany()
            .HasForeignKey(d => d.EscrowId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.OpenedByUser)
            .WithMany()
            .HasForeignKey(d => d.OpenedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.ReviewedByAdmin)
            .WithMany()
            .HasForeignKey(d => d.ReviewedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

