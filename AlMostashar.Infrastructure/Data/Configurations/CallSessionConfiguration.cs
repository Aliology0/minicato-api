using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class CallSessionConfiguration : IEntityTypeConfiguration<CallSession>
{
    public void Configure(EntityTypeBuilder<CallSession> builder)
    {
        // ─── Table ───
        builder.ToTable("CallSessions");

        // ─── Properties ───
        builder.Property(cs => cs.ChannelName)
            .IsRequired()
            .HasColumnType("varchar")
            .HasMaxLength(100);

        builder.Property(cs => cs.CallType)
            .IsRequired()
            .HasConversion<string>()
            .HasColumnType("varchar")
            .HasMaxLength(20);

        builder.Property(cs => cs.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasColumnType("varchar")
            .HasMaxLength(20);

        builder.Property(cs => cs.StartTime)
            .IsRequired();

        // ─── Relationships ───

        // N:1 — CallSession → Chat (the only FK relationship — calls are scoped to chats)
        builder.HasOne(cs => cs.Chat)
            .WithMany(cs=>cs.CallSessions)
            .HasForeignKey(cs => cs.ChatId)
            .OnDelete(DeleteBehavior.Restrict);

        // CallerId and ReceiverId are plain int columns (no FK to User).
        // They record who initiated and who received, but the relationship
        // is through Chat → ChatParticipants per the business rule.

        // ─── Indexes ───
        builder.HasIndex(cs => cs.ChannelName)
            .IsUnique();

        builder.HasIndex(cs => cs.ChatId);
        builder.HasIndex(cs => cs.CallerId);
        builder.HasIndex(cs => cs.ReceiverId);

        // Composite index for the "find active call" query used in reconnection check
        builder.HasIndex(cs => new { cs.ChatId, cs.Status });

        // Unique filtered index: only ONE active call (Initiated or Ongoing) per chat at a time.
        // Prevents duplicate active sessions caused by concurrent requests (TOCTOU race).
        builder.HasIndex(cs => cs.ChatId)
            .HasDatabaseName("IX_CallSessions_ChatId_ActiveCall")
            .IsUnique()
            .HasFilter("[Status] IN ('Initiated', 'Ongoing')");
    }
}
