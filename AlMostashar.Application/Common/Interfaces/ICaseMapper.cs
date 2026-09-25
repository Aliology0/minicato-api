using AlMostashar.Application.Features.Cases.Services;
using AlMostashar.Domain.Entities;

namespace AlMostashar.Application.Common.Interfaces;

public interface ICaseMapper
{
    Case Create(CaseCreationData data);
}
