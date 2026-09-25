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
    public class GetPresignedUrlByPathCommand : IRequest<Result<GetPresignedUrlByPathResponseDto>>
    {
        public string FilePath { get; set; } = null!;
        public int? ExpirationMinutes { get; set; }
    }
}
