using AlMostashar.Application.Common.Enums;

namespace AlMostashar.Application.Common.Interfaces
{
    public interface IStorageService
    {
        /// <summary>
        /// Uploads a file and returns the file key (path) to store in the database.
        /// </summary>
        Task<string> UploadFileAsync(Stream fileStream, string fileName, FileContentType contentType);

        /// <summary>
        /// Uploads a file to a public bucket and returns the file key (path) to store in the database.
        /// </summary>
        Task<string> UploadPublicFileAsync(Stream fileStream, string fileName, FileContentType contentType);

        /// <summary>
        /// Generates a secure, temporary presigned URL for downloading a file.
        /// </summary>
        string GetPresignedUrl(string filePath, int expirationMinutes = 60);

        /// <summary>
        /// Generates a permanent, publicly accessible URL for a file.
        /// Requires the storage bucket/container to allow public read access on the file path.
        /// </summary>
        string GetPublicUrl(string fileKey);

        /// <summary>
        /// Deletes a file from storage by its key.
        /// </summary>
        Task<(bool Success, string Message)> DeleteFileAsync(string filePath);
    }
}
