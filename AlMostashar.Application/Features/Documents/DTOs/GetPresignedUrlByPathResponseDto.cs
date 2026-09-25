using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlMostashar.Application.Features.Documents.DTOs
{
    public class GetPresignedUrlByPathResponseDto
    {
        public string Url { get; set; } = null!;
        public int ExpirationMinutes { get; set; }
    }
}
