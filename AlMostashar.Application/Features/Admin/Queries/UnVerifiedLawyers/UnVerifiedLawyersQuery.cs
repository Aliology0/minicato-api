using AlMostashar.Application.Features.Admin.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlMostashar.Application.Features.Admin.Queries.UnVerifiedLawyers
{
    public class UnVerifiedLawyersQuery:IRequest<Result<List<UnVerifiedLawyersDto>>>
    {
    }
}
