using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Documents.Commands.GetPresignedUrl;
using AlMostashar.Application.Features.Documents.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlMostashar.Application.Features.Documents.Commands.GetPresignedUrlByPath
{
    public class GetPresignedUrlByPathCommandHandler : IRequestHandler<GetPresignedUrlByPathCommand, Result<GetPresignedUrlByPathResponseDto>>
    {
        private readonly IStorageService _storageService;

        public GetPresignedUrlByPathCommandHandler(IStorageService storageService)
        {
            _storageService = storageService;
        }

        public Task<Result<GetPresignedUrlByPathResponseDto>> Handle(
            GetPresignedUrlByPathCommand request,
            CancellationToken cancellationToken)
        {
            var expiration = request.ExpirationMinutes ?? 60;
            var url = _storageService.GetPresignedUrl(request.FilePath, expiration);

            var response = new GetPresignedUrlByPathResponseDto
            {
                Url = url,
                ExpirationMinutes = expiration
            };

            return Task.FromResult(Result<GetPresignedUrlByPathResponseDto>.Success(response));
        }
    }
}
