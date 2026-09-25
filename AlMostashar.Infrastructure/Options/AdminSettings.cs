namespace AlMostashar.Infrastructure.Options;

public sealed class AdminSettings
{
    public const string SectionName = "AdminSettings";

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = "System";
    public string LastName { get; set; } = "Admin";
}
