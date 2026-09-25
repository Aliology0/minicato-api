namespace AlMostashar.Application.Features.Auth.DTOs
{
    /// <summary>
    /// Additional user details returned for Lawyer accounts.
    /// For Client accounts, this will be null.
    /// </summary>
    public class UserDetailsDto
    {
        public string? Governorate { get; set; }
        public string? City { get; set; }
        public string? Bio { get; set; }
        public string? About { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
