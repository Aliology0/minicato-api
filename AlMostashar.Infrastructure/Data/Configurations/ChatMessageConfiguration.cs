using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        // ─── Table ───
        builder.ToTable("ChatMessages");

        // ─── Properties ───
        builder.Property(m => m.MessageType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(m => m.Content)
            .HasMaxLength(4000);

        // ─── Indexes ───
        builder.HasIndex(m => m.ChatId);
        builder.HasIndex(m => m.SenderId);
        builder.HasIndex(m => m.CaseDocumentId)
            .IsUnique();

        // Composite index to optimize "MarkMessagesAsReadAndResetCountAsync" query
        builder.HasIndex(m => new { m.ChatId, m.IsRead, m.SenderId, m.Id })
            .HasDatabaseName("IX_ChatMessages_MarkAsReadCovering");

        // ─── Relationships ───

        // N:1 — ChatMessage → User (Sender)
        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        // 1:1 — ChatMessage ↔ CaseDocuments
        builder.HasOne(m => m.CaseDocument)
            .WithOne(d => d.ChatMessage)
            .HasForeignKey<ChatMessage>(m => m.CaseDocumentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
