using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.Admin.DTOs;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;

namespace AlMostashar.Application.Features.Admin.Queries.GetUsers;

public class GetUsersQuery : IRequest<Result<CursorPagedResult<UserListItemDto>>>
{
    public UserRole? Role { get; set; }
    public AccountStatus? AccountStatus { get; set; }
    public bool? IsVerified { get; set; }
    public string? Search { get; set; }

    /// <summary>Id of the last item from the previous page.</summary>
    public int? Cursor { get; set; }

    /// <summary>CreatedAt of the last item from the previous page (used together with Cursor for tie-breaking).</summary>
    public DateTime? CursorDate { get; set; }

    public int PageSize { get; set; } = 20;
}

