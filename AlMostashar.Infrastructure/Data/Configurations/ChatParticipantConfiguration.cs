using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class ChatParticipantConfiguration : IEntityTypeConfiguration<ChatParticipant>
{
    public void Configure(EntityTypeBuilder<ChatParticipant> builder)
    {
        // ─── Table ───
        builder.ToTable("ChatParticipants");

        // ─── Composite Primary Key ───
        builder.HasKey(cp => new { cp.ChatId, cp.UserId });

        // ─── Indexes ───
        builder.HasIndex(cp => cp.ChatId);
        builder.HasIndex(cp => cp.UserId);
    }
}
