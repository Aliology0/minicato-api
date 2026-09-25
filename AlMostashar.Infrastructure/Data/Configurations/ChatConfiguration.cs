using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class ChatConfiguration : IEntityTypeConfiguration<Chat>
{
    public void Configure(EntityTypeBuilder<Chat> builder)
    {
        // ─── Table ───
        builder.ToTable("Chats");

        // ─── Properties ───
        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasColumnType("varchar")
            .HasMaxLength(50);

        builder.Property(c=>c.LastMessageType)
            .HasConversion<string>()
            .HasColumnType("varchar")
            .HasMaxLength(50);

        // ─── Relationships ───

        // 1:N — Chat → ChatMessage
        builder.HasMany(c => c.ChatMessages)
            .WithOne(m => m.Chat)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1:N — Chat → ChatParticipant
        builder.HasMany(c => c.ChatParticipants)
            .WithOne(cp => cp.Chat)
            .HasForeignKey(cp => cp.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        // ─── Indexes ───
        builder.HasIndex(c => c.CaseId)
            .IsUnique();

        builder.HasIndex(c => c.LastMessageAt)
            .IsDescending();

        // 1:1 Total — Chat → Case (every Chat must have a Case)
        builder.HasOne(c => c.Case)
            .WithOne(cs => cs.Chat)
            .HasForeignKey<Chat>(c => c.CaseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
