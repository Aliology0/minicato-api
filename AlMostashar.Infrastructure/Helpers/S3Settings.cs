namespace AlMostashar.Infrastructure.Helpers
{
    public class S3Settings
    {
        public string BucketName { get; set; } = null!;
        public string Region { get; set; } = null!;
        public string Endpoint { get; set; } = null!;
        public string AccessKey { get; set; } = null!;
        public string SecretKey { get; set; } = null!;
        public string PublicBucketName { get; set; } = null!;
    }
}
