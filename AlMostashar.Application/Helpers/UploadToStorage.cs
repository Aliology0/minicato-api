using AlMostashar.Application.Common.Helpers;
using AlMostashar.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AlMostashar.Application.Helpers
{
    public static class UploadToStorage
    {
        public static async Task<string> UploadAsync(IFormFile file, IStorageService _storageService)
        {
            var extension = Path.GetExtension(file.FileName);
            var contentType = FileContentTypeHelper.FromExtension(extension)
                ?? throw new InvalidOperationException($"Unsupported file extension: {extension}");

            using var stream = file.OpenReadStream();
            return await _storageService.UploadFileAsync(stream, file.FileName, contentType);
        }

        public static async Task<string> UploadPublicAsync(IFormFile file, IStorageService _storageService)
        {
            var extension = Path.GetExtension(file.FileName);
            var contentType = FileContentTypeHelper.FromExtension(extension)
                ?? throw new InvalidOperationException($"Unsupported file extension: {extension}");

            using var stream = file.OpenReadStream();
            return await _storageService.UploadPublicFileAsync(stream, file.FileName, contentType);
        }

    }
}
