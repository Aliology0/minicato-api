using System.Net;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Enums;
using AlMostashar.Infrastructure.Helpers;
using Amazon.S3;
using Amazon.S3.Model;

namespace AlMostashar.Infrastructure.Services
{
    public class S3StorageService : IStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly S3Settings _settings;

        public S3StorageService(IAmazonS3 s3Client, S3Settings settings)
        {
            _s3Client = s3Client;
            _settings = settings;
        }

        /// <summary>
        /// Decodes and trims the file key to handle URL-encoded paths (e.g. spaces as %20).
        /// </summary>
        private bool isImage(FileContentType contentType)
            => contentType == FileContentType.Png || contentType == FileContentType.Jpeg || contentType == FileContentType.Jpg || contentType == FileContentType.Heic || contentType == FileContentType.Webp;

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, FileContentType contentType)
        {
            return await UploadToS3Async(fileStream, fileName, contentType, _settings.BucketName);
        }

        public async Task<string> UploadPublicFileAsync(Stream fileStream, string fileName, FileContentType contentType)
        {
            return await UploadToS3Async(fileStream, fileName, contentType, _settings.PublicBucketName);
        }

        private async Task<string> UploadToS3Async(Stream fileStream, string fileName, FileContentType contentType, string bucketName)
        {
            string fileKey;
            if(isImage(contentType))
                fileKey = $"images/{Guid.NewGuid()}";
            else
                fileKey = $"documents/{Guid.NewGuid()}";

            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = fileKey,
                InputStream = fileStream,
                ContentType = contentType.ToMimeType()
            };

            try
            {
                await _s3Client.PutObjectAsync(request);
                return fileKey;
            }
            catch (AmazonS3Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to upload file '{fileName}' to S3 bucket '{bucketName}'.", ex);
            }
        }

        public string GetPresignedUrl(string filePath, int expirationMinutes = 60)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _settings.BucketName,
                Key = filePath,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };

            return _s3Client.GetPreSignedURL(request);
        }

        public string GetPublicUrl(string fileKey)
        {
            // Build a direct public URL: https://{endpoint}/{bucket}/{key}
            var publicEndpoint = _settings.Endpoint
                    .TrimEnd('/')
                    .Replace(".storage.supabase.co/storage/v1/s3", ".supabase.co/storage/v1/object/public");
            return $"{publicEndpoint}/{_settings.PublicBucketName}/{fileKey}";
        }

        public async Task<(bool Success, string Message)> DeleteFileAsync(string filePath)
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = filePath
            };

            try
            {
                var response = await _s3Client.DeleteObjectAsync(request);
                return response.HttpStatusCode == System.Net.HttpStatusCode.OK
                    ? (true, "File deleted successfully.")
                    : (false, $"Failed to delete file. S3 returned status code: {response.HttpStatusCode}.");
            }
            catch (AmazonS3Exception ex)
            {
                return (false, $"S3 error while deleting file '{filePath}': {ex.Message}");
            }
        }
    }
}
