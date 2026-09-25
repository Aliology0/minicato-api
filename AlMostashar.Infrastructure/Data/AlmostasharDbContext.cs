using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Entities;
using Amazon.Runtime.Internal.UserAgent;
using MediatR;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AlMostashar.Infrastructure.Data
{
    public class AlmostasharDbContext : DbContext, IAppDbContext, IDataProtectionKeyContext
    {
        private readonly IPublisher _publisher;
        
        public AlmostasharDbContext(DbContextOptions<AlmostasharDbContext> options, IPublisher publisher) : base(options)
        {
            _publisher = publisher;
        }

        // ─── DbSets ───
        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Lawyer> Lawyers { get; set; }
        public DbSet<LawyerSpecialization> LawyerSpecializations { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<CaseDocuments> CaseDocuments { get; set; }
        public DbSet<CaseNotes> CaseNotes { get; set; }
        public DbSet<CaseTimeline> CaseTimelines { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatParticipant> ChatParticipants { get; set; }
        public DbSet<ClientRequest> ClientRequests { get; set; }
        public DbSet<ConsultationRequestDetails> ConsultationRequestDetails { get; set; }
        public DbSet<ContractRequestDetails> ContractRequestDetails { get; set; }
        public DbSet<LawsuitRequestDetails> LawsuitRequestDetails { get; set; }
        public DbSet<CompanyFormationRequestDetails> CompanyFormationRequestDetails { get; set; }
        public DbSet<GenericRequestDetails> GenericRequestDetails { get; set; }
        public DbSet<RequestOffer> RequestOffers { get; set; }
        public DbSet<BroadcastRequest> BroadcastRequests { get; set; }
        public DbSet<DirectRequest> DirectRequests { get; set; }
        public DbSet<Escrow> Escrows { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<LawyerService> LawyerServices { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Dispute> Disputes { get; set; }
        public DbSet<LegalService> LegalServices { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<WithdrawalRequest> WithdrawalRequests { get; set; }
        public DbSet<PaymentWalletTransaction> PaymentWalletTransactions { get; set; }
        public DbSet<CaseClientRequest> CaseClientRequests { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<UserFcmToken> UserFcmTokens { get; set; }
        public DbSet<CallSession> CallSessions { get; set; }
        public DbSet<WebhookLog> WebhookLogs { get; set; }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

        public Task<int> ConsumePasswordResetTokenAsync(string tokenId, int userId, DateTime nowUtc, CancellationToken cancellationToken)
        {
            return Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE [PasswordResetTokens]
                SET [IsUsed] = 1,
                    [UsedAt] = {nowUtc}
                WHERE [TokenId] = {tokenId}
                  AND [UserId] = {userId}
                  AND [IsUsed] = 0
                  AND [ExpiresAt] > {nowUtc}", cancellationToken);
        }

        public async Task<bool> TryClaimUnlinkedDocumentAsync(int documentId, DateTime olderThanUtc, string claimToken, DateTime claimedAtUtc, CancellationToken cancellationToken)
        {
            var affected = await CaseDocuments
                .Where(x => x.Id == documentId && x.ClientRequestId == null && x.CaseId == null && x.ReportId == null && x.CleanupClaimToken == null && x.CreatedAt < olderThanUtc)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.CleanupClaimToken, claimToken)
                    .SetProperty(x => x.CleanupClaimedAt, claimedAtUtc), cancellationToken);

            if (affected == 1)
            {
                var tracked = CaseDocuments.Local.FirstOrDefault(x => x.Id == documentId);
                if (tracked is not null)
                {
                    tracked.CleanupClaimToken = claimToken;
                    tracked.CleanupClaimedAt = claimedAtUtc;
                    Entry(tracked).Property(x => x.CleanupClaimToken).OriginalValue = claimToken;
                    Entry(tracked).Property(x => x.CleanupClaimedAt).OriginalValue = claimedAtUtc;
                }
            }

            return affected == 1;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all IEntityTypeConfiguration<T> classes from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var result = await base.SaveChangesAsync(cancellationToken);

            await PublishDomainEventsAsync(cancellationToken);

            return result;
        }

        private async Task PublishDomainEventsAsync(CancellationToken cancellationToken)
        {
            const int maxIterations = 10;
            var iteration = 0;

            // Loop to handle cascading events (events raised by event handlers)
            while (iteration++ < maxIterations)
            {
                var entitiesWithEvents = ChangeTracker.Entries<BaseEntity>()
                    .Select(e => e.Entity)
                    .Where(e => e.DomainEvents.Any())
                    .ToList();

                if (entitiesWithEvents.Count == 0)
                    break;

                var domainEvents = entitiesWithEvents
                    .SelectMany(e => e.DomainEvents)
                    .ToList();

                // Clear events before publishing to avoid re-publishing
                foreach (var entity in entitiesWithEvents)
                {
                    entity.ClearDomainEvents();
                }

                foreach (var domainEvent in domainEvents)
                {
                    await _publisher.Publish(domainEvent, cancellationToken);
                }
            }
        }

        /// <inheritdoc />
        public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            var strategy = Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async (ct) =>
            {
                await using var transaction = await Database.BeginTransactionAsync(ct);

                try
                {
                    await action(ct);
                    await transaction.CommitAsync(ct);

                }
                catch
                {
                    await transaction.RollbackAsync(ct);
                    throw;
                }
            }, cancellationToken);
        }

        public async Task<int> MarkMessagesAsReadAndResetCountAsync(int chatId, int lastReadMessageId, int currentUserId, CancellationToken cancellationToken = default)
        {
            var nowUtc = DateTime.UtcNow;

            var result = await Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE [ChatMessages]
                SET [IsRead] = 1,
                    [ReadAt] = {nowUtc}
                WHERE [ChatId] = {chatId}
                    AND [IsRead] = 0
                    AND [Id] <= {lastReadMessageId}
                    AND [SenderId] != {currentUserId};
                UPDATE [ChatParticipants]
                SET [UnReadMessageCount] = 0
                WHERE [ChatId] = {chatId}
                  AND [UserId] = {currentUserId};
            ", cancellationToken);

            return result;
        }

        public async Task<int> MarkAllNotificationsAsReadAsync(int userId, int? lastReadId, CancellationToken cancellationToken = default)
        {
            var query = Notifications.Where(n => n.UserId == userId && !n.IsRead);

            if (lastReadId.HasValue)
            {
                query = query.Where(n => n.Id <= lastReadId.Value);
            }

            return await query.ExecuteUpdateAsync(
                setters => setters.SetProperty(n => n.IsRead, true),
                cancellationToken);
        }
    }
}
