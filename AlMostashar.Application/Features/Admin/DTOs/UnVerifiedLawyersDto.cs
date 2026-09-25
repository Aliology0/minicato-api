using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlMostashar.Application.Features.Admin.DTOs;

public record UnVerifiedLawyersDto(
    int Id,
    string FullName,
    DateTime CreatedAt,
    string PhoneNo,
    int SyndicateId,
    string? SSN_Url,
    string? SyndicateCardUrl,
    string? PracticeCertificatesUrl);


