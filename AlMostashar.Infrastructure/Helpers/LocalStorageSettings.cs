namespace AlMostashar.Infrastructure.Helpers
{
    public class LocalStorageSettings
    {
        // Relative to content root or absolute
        public string PublicPath { get; set; } = "wwwroot/uploads/avatars";
        public string PrivatePath { get; set; } = "App_Data/uploads/private";
        // Optional explicit public base URL (e.g., https://api.example.com). If empty, builder will derive from requests.
        public string? PublicBaseUrl { get; set; }
        // Max allowed file size in bytes (optional)
        public long MaxFileSizeBytes { get; set; } = 20 * 1024 * 1024; // 20 MB default
    }
}
