using System.Globalization;
using System.Resources;

namespace AlMostashar.Application.Common.Constants
{
    /// <summary>
    /// Centralized messages with multi-language support via .resx resource files.
    /// Language is determined by <see cref="CultureInfo.CurrentUICulture"/>.
    /// Resource files: Common/Resources/Messages.resx (EN), Messages.ar.resx (AR)
    /// </summary>
    public static class Messages
    {
        private static readonly ResourceManager _rm =
            new("AlMostashar.Application.Common.Resources.Messages",
                typeof(Messages).Assembly);

        internal static string Get(string key) => _rm.GetString(key, CultureInfo.CurrentUICulture) ?? key;

        // ── Auth: Errors ─────────────────────────────────────────────────────
        public static class Auth
        {
            public static string InvalidCredentials     => Get("Auth_InvalidCredentials");
            public static string EmailNotVerified        => Get("Auth_EmailNotVerified");
            public static string EmailAlreadyRegistered  => Get("Auth_EmailAlreadyRegistered");
            public static string SyndicateIdAlreadyRegistered => Get("Auth_SyndicateIdAlreadyRegistered");
            public static string AlreadyVerified         => Get("Auth_AlreadyVerified");
            public static string InvalidOtp              => Get("Auth_InvalidOtp");
            public static string OtpExpired              => Get("Auth_OtpExpired");
            public static string InvalidResetToken       => Get("Auth_InvalidResetToken");
            public static string UserNotFound            => Get("Auth_UserNotFound");
            public static string InvalidRefreshToken     => Get("Auth_InvalidRefreshToken");
            public static string NotCaseOwner            => Get("Auth_NotCaseOwner");
            public static string CannotUploadToCase      => Get("Auth_CannotUploadToCase");
            public static string CannotViewCase          => Get("Auth_CannotViewCase");
            public static string NotChatParticipant      => Get("Auth_NotChatParticipant");
            public static string OnlyLawyersCanSendOffers=> Get("Auth_OnlyLawyersCanSendOffers");
        }

        // ── Auth: Success ────────────────────────────────────────────────────
        public static class AuthSuccess
        {
            public static string OtpSent                 => Get("Auth_OtpSent");
            public static string PasswordReset           => Get("Auth_PasswordReset");
            public static string VerificationSent        => Get("Auth_VerificationSent");
            public static string ResendSuccess           => Get("Auth_ResendSuccess");
        }

        // ── General: Success ─────────────────────────────────────────────────
        public static class GeneralSuccess
        {
            public static string FeedbackSentSuccessfully => Get("Success_FeedbackSent");
            public static string ReportSubmitted          => Get("Success_ReportSubmitted");
            public static string DocumentDeleted         => Get("Success_DocumentDeleted");
            public static string NoteDeleted             => Get("Success_NoteDeleted");
            public static string OfferAccepted           => Get("Success_OfferAccepted");
            public static string RequestAccepted         => Get("Success_RequestAccepted");
            public static string RequestCanceled         => Get("Success_RequestCanceled");
            public static string InvoicePaid             => Get("Success_InvoicePaid");
            public static string OfferRejected           => Get("Success_OfferRejected");
            public static string RequestRejected         => Get("Success_RequestRejected");
        }

        // ── Validation ───────────────────────────────────────────────────────
        public static class Validation
        {
            public static string FirstNameRequired       => Get("Validation_FirstNameRequired");
            public static string FirstNameMaxLength      => Get("Validation_FirstNameMaxLength");
            public static string LastNameRequired        => Get("Validation_LastNameRequired");
            public static string LastNameMaxLength       => Get("Validation_LastNameMaxLength");
            public static string EmailRequired           => Get("Validation_EmailRequired");
            public static string EmailInvalid            => Get("Validation_EmailInvalid");
            public static string PasswordRequired        => Get("Validation_PasswordRequired");
            public static string PasswordMinLength       => Get("Validation_PasswordMinLength");
            public static string PasswordUppercase       => Get("Validation_PasswordUppercase");
            public static string PasswordLowercase       => Get("Validation_PasswordLowercase");
            public static string PasswordDigit           => Get("Validation_PasswordDigit");
            public static string PhoneRequired           => Get("Validation_PhoneRequired");
            public static string PhoneInvalid            => Get("Validation_PhoneInvalid");
            public static string GovernorateRequired     => Get("Validation_GovernorateRequired");
            public static string GovernorateInvalid      => Get("Validation_GovernorateInvalid");
            public static string CityRequired            => Get("Validation_CityRequired");
            public static string GovernorateLookupNotFound => Get("Validation_GovernorateLookupNotFound");
            public static string CityLookupNotFound        => Get("Validation_CityLookupNotFound");
            public static string CityGovernorateMismatch   => Get("Validation_CityGovernorateMismatch");
            public static string SyndicateIdRequired     => Get("Validation_SyndicateIdRequired");
            public static string OtpRequired             => Get("Validation_OtpRequired");
            public static string OtpLength               => Get("Validation_OtpLength");
            public static string ResetTokenRequired      => Get("Validation_ResetTokenRequired");
            public static string NewPasswordRequired     => Get("Validation_NewPasswordRequired");
            public static string RefreshTokenRequired    => Get("Validation_RefreshTokenRequired");
            public static string UserIdRequired          => Get("Validation_UserIdRequired");
            public static string AtLeastOneFileRequired  => Get("Validation_AtLeastOneFileRequired");
            public static string FileTooLarge            => Get("Validation_FileTooLarge");
            public static string FileTypeNotAllowed      => Get("Validation_FileTypeNotAllowed");
            public static string FileRequired            => Get("Validation_FileRequired");
            public static string FileEmpty               => Get("Validation_FileEmpty");
            public static string ProfilePictureRequired  => Get("Validation_ProfilePictureRequired");
            public static string NationalIdPhotoRequired => Get("Validation_NationalIdPhotoRequired");
            public static string SsnRequired             => Get("Validation_SsnRequired");
            public static string SyndicateCardRequired   => Get("Validation_SyndicateCardRequired");
            public static string RatingMustBeBetween1And5 => Get("Validation_RatingMustBeBetween1And5");
        }

        // ── Admin ────────────────────────────────────────────────────────────
        public static class Admin
        {
            public static string LawyerNotFound          => Get("Admin_LawyerNotFound");
            public static string LawyerAlreadyVerified   => Get("Admin_LawyerAlreadyVerified");
            public static string LawyerVerified          => Get("Admin_LawyerVerified");
            public static string UserAlreadyVerified     => Get("Admin_UserAlreadyVerified");
            public static string UserAlreadyRejected     => Get("Admin_UserAlreadyRejected");
            public static string CannotVerifyAdmin       => Get("Admin_CannotVerifyAdmin");
        }

        // ── Cases ────────────────────────────────────────────────────────────
        public static class Cases
        {
            public static string NotFound                => Get("Cases_NotFound");
            public static string DocumentNotFound        => Get("Cases_DocumentNotFound");
            public static string NoteNotFound            => Get("Cases_NoteNotFound");
        }

        // ── Invoices ─────────────────────────────────────────────────────────
        public static class Invoices
        {
            public static string Unauthorized          => Get("Invoices_Unauthorized");
            public static string AlreadyPaid           => Get("Invoices_AlreadyPaid");
            public static string RequestNotAccepted    => Get("Invoices_RequestNotAccepted");
        }

        // ── Chat ─────────────────────────────────────────────────────────────
        public static class Chat
        {
            public static string NotFound                => Get("Chat_NotFound");
            public static string OtherParticipantNotFound=> Get("Chat_OtherParticipantNotFound");
        }

        // ── Call ─────────────────────────────────────────────────────────────
        public static class Call
        {
            public static string Accepted                => Get("Call_Accepted");
            public static string Rejected                => Get("Call_Rejected");
            public static string Ended                   => Get("Call_Ended");
            public static string NotFound                => Get("Call_NotFound");
            public static string NotParticipant          => Get("Call_NotParticipant");
            public static string InvalidTransition       => Get("Call_InvalidTransition");
        }

        // ── Offers & Requests ────────────────────────────────────────────────
        public static class Offers
        {
            public static string NotYourRequest          => Get("Offers_NotYourRequest");
            public static string OnlyPendingCanBeAccepted=> Get("Offers_OnlyPendingCanBeAccepted");
            public static string OnlyPendingCanBeRejected=> Get("Offers_OnlyPendingCanBeRejected");
            public static string CannotAcceptNonPending  => Get("Offers_CannotAcceptNonPending");
            public static string CannotRejectNonPending  => Get("Offers_CannotRejectNonPending");
            public static string CannotSendNonPending    => Get("Offers_CannotSendNonPending");
            public static string PendingOfferExists      => Get("Offers_PendingOfferExists");
            public static string UnsupportedRequestType  => Get("Offers_UnsupportedRequestType");
        }

        // ── Generic (reusable, parameterized templates) ──────────────────────
        public static class Generic
        {
            public static string Required(string field)
                => string.Format(Get("Generic_Required"), field);

            public static string NotFound(string entity)
                => string.Format(Get("Generic_NotFound"), entity);

            public static string AlreadyExists(string entity)
                => string.Format(Get("Generic_AlreadyExists"), entity);

            public static string NotVerified(string entity)
                => string.Format(Get("Generic_NotVerified"), entity);

            public static string MaxLength(string field, int max)
                => string.Format(Get("Generic_MaxLength"), field, max);

            public static string Negative(string field)
                => string.Format(Get("Generic_Negative"), field);

            public static string InvalidId(string field)
                => string.Format(Get("Generic_InvalidId"), field);

            public static string InvalidEnumValue(string field)
                => string.Format(Get("Generic_InvalidEnumValue"), field);
        }

        // ── Enums ────────────────────────────────────────────────────────────
        public static class Enums
        {
            public static string GetServiceType(AlMostashar.Domain.ValueObject.Enum.ServiceType serviceType)
                => Get($"Enum_ServiceType_{serviceType}");

            public static string GetCaseStatus(AlMostashar.Domain.ValueObject.Enum.CaseStatus status)
                => Get($"Enum_CaseStatus_{status}");
        }

        // ── Localized field display names ────────────────────────────────────
        public static class Fields
        {
            public static string ServiceId              => Get("Field_ServiceId");
            public static string Price                   => Get("Field_Price");
            public static string Duration                => Get("Field_Duration");
            public static string Lawyer                  => Get("Field_Lawyer");
            public static string Service                 => Get("Field_Service");
            public static string ServiceTitle            => Get("Field_ServiceTitle");
            public static string ServiceSummary          => Get("Field_ServiceSummary");
            public static string ServiceFullDescription  => Get("Field_ServiceFullDescription");
            public static string ServiceType             => Get("Field_ServiceType");
            public static string ExpectedDuration        => Get("Field_ExpectedDuration");
            public static string SSN                     => Get("Field_SSN");
            public static string SyndicateCard            => Get("Field_SyndicateCard");
            public static string PracticeCertificates     => Get("Field_PracticeCertificates");
            public static string FilePath                 => Get("Field_FilePath");
            public static string CaseId                  => Get("Field_CaseId");
            public static string CaseTitle               => Get("Field_CaseTitle");
            public static string CaseDescription         => Get("Field_CaseDescription");
            public static string CaseStatus              => Get("Field_CaseStatus");
            public static string NoteContent             => Get("Field_NoteContent");
            public static string TimelineTitle           => Get("Field_TimelineTitle");
            public static string TimelineContent         => Get("Field_TimelineContent");
            public static string DocumentName            => Get("Field_DocumentName");
            public static string DocumentUrl             => Get("Field_DocumentUrl");
            public static string FeedbackRating          => Get("Field_FeedbackRating");
            public static string FeedbackContent         => Get("Field_FeedbackContent");
        }

        // ── Notifications ────────────────────────────────────────────────────
        public static class Notifications
        {
            public static string RequestAcceptedTitle    => Get("Notification_RequestAccepted_Title");
            public static string RequestAcceptedBody(string lawyerName)
                => string.Format(Get("Notification_RequestAccepted_Body"), lawyerName);
            public static string NewDirectRequestTitle   => Get("Notification_NewDirectRequest_Title");
            public static string NewDirectRequestBody
                => Get("Notification_NewDirectRequest_Body");
            public static string CaseStartedTitle        => Get("Notification_CaseStarted_Title");
            public static string CaseStartedBody(string clientName)
                => string.Format(Get("Notification_CaseStarted_Body"), clientName);
            public static string ChatAvailableTitle      => Get("Notification_ChatAvailable_Title");
            public static string ChatAvailableBody(string lawyerName)
                => string.Format(Get("Notification_ChatAvailable_Body"), lawyerName);
            public static string NewMessageTitle         => Get("Notification_NewMessage_Title");
            public static string NewMessageBody(string senderName)
                => string.Format(Get("Notification_NewMessage_Body"), senderName);
            public static string NewServiceTitle         => Get("Notification_NewService_Title");
            public static string NewServiceBody(string serviceName)
                => string.Format(Get("Notification_NewService_Body"), serviceName);
            public static string CaseCompletedTitle      => Get("Notification_CaseCompleted_Title");
            public static string CaseCompletedBody(string clientName, string caseTitle)
                => string.Format(Get("Notification_CaseCompleted_Body"), clientName, caseTitle);
            public static string CaseStatusUpdatedTitle  => Get("Notification_CaseStatusUpdated_Title");
            public static string CaseStatusUpdatedBody(string caseTitle, string statusName)
                => string.Format(Get("Notification_CaseStatusUpdated_Body"), caseTitle, statusName);
        }
    }
}
