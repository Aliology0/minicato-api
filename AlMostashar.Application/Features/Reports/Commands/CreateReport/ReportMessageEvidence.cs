namespace AlMostashar.Application.Features.Reports.Commands.CreateReport;

/// <summary>
/// A single chat message submitted by the reporter as evidence.
/// Since messages are end-to-end encrypted and only decryptable on the user's device,
/// the frontend decrypts and sends them directly from its local database.
/// </summary>
public class ReportMessageEvidence
{
    /// <summary>Optional. Original ID of the message if available.</summary>
    public string? MessageId { get; set; }

    /// <summary>Optional. ID of the chat thread.</summary>
    public string? ChatId { get; set; }

    /// <summary>Optional. ID of the user who sent the message.</summary>
    public int? SenderUserId { get; set; }

    /// <summary>Display name of the message sender (as shown in the chat).</summary>
    public string SenderName { get; set; } = default!;

    /// <summary>The plaintext message content after local decryption.</summary>
    public string Content { get; set; } = default!;

    /// <summary>UTC timestamp when the message was originally sent.</summary>
    public DateTime SentAt { get; set; }
}
