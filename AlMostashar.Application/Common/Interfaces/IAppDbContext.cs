using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace AlMostashar.Application.Common.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Client> Clients { get; }
        DbSet<Lawyer> Lawyers { get; }
        DbSet<LawyerSpecialization> LawyerSpecializations { get; }
        DbSet<RefreshToken> RefreshTokens { get; }
        DbSet<PasswordResetToken> PasswordResetTokens { get; }
        DbSet<UserFcmToken> UserFcmTokens { get; }
        public DbSet<Admin> Admins { get; }
        public DbSet<Case> Cases { get; }
        public DbSet<CaseDocuments> CaseDocuments { get; }
        public DbSet<CaseNotes> CaseNotes { get; }
        public DbSet<CaseTimeline> CaseTimelines { get; }
        public DbSet<Chat> Chats { get; }
        public DbSet<ChatMessage> ChatMessages { get; }
        public DbSet<ChatParticipant> ChatParticipants { get; }
        public DbSet<ClientRequest> ClientRequests { get; }
        public DbSet<ConsultationRequestDetails> ConsultationRequestDetails { get; }
        public DbSet<ContractRequestDetails> ContractRequestDetails { get; }
        public DbSet<LawsuitRequestDetails> LawsuitRequestDetails { get; }
        public DbSet<CompanyFormationRequestDetails> CompanyFormationRequestDetails { get; }
        public DbSet<GenericRequestDetails> GenericRequestDetails { get; }
        public DbSet<RequestOffer> RequestOffers { get; }
        public DbSet<BroadcastRequest> BroadcastRequests { get; }
        public DbSet<DirectRequest> DirectRequests { get; }
        public DbSet<Escrow> Escrows { get; }
        public DbSet<Feedback> Feedbacks { get; }
        public DbSet<Invoice> Invoices { get; }
        public DbSet<LawyerService> LawyerServices { get; }
        public DbSet<Notification> Notifications { get; }
        public DbSet<Payment> Payments { get; }
        public DbSet<Report> Reports { get; }
        public DbSet<Dispute> Disputes { get; }
        public DbSet<LegalService> LegalServices { get; }
        public DbSet<Wallet> Wallets { get; }
        public DbSet<WalletTransaction> WalletTransactions { get; }
        public DbSet<WithdrawalRequest> WithdrawalRequests { get; }
        public DbSet<PaymentWalletTransaction> PaymentWalletTransactions { get; }
        public DbSet<CaseClientRequest> CaseClientRequests { get; }
        public DbSet<CallSession> CallSessions { get; }
        public DbSet<WebhookLog> WebhookLogs { get; }

        Task<int> ConsumePasswordResetTokenAsync(string tokenId, int userId, DateTime nowUtc, CancellationToken cancellationToken);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Executes the given action within a database transaction,
        /// respecting the configured execution strategy (e.g. retry on failure).
        /// </summary>
        Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken);
        Task<int> MarkMessagesAsReadAndResetCountAsync(int chatId, int lastReadMessageId, int currentUserId, CancellationToken cancellationToken = default);
        Task<int> MarkAllNotificationsAsReadAsync(int userId, int? lastReadId, CancellationToken cancellationToken = default);
        Task<bool> TryClaimUnlinkedDocumentAsync(int documentId, DateTime olderThanUtc, string claimToken, DateTime claimedAtUtc, CancellationToken cancellationToken);
    }
}

