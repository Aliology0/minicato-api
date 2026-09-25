using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.Admin.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Admin.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<CursorPagedResult<UserListItemDto>>>
{
    private readonly IAppDbContext _db;

    public GetUsersQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<CursorPagedResult<UserListItemDto>>> Handle(
        GetUsersQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var normalizedSearch = request.Search?.Trim().ToLower();

        // ── Base: query the Users set (TPH — all users live in one table) ────
        var query = _db.Users.AsNoTracking().AsQueryable();

        // ── Role filter ───────────────────────────────────────────────────────
        query = request.Role switch
        {
            UserRole.Client => query.Where(u => u is Client),
            UserRole.Lawyer => query.Where(u => u is Lawyer),
            _ => query.Where(u => u is Client || u is Lawyer) // exclude Admins
        };

        // ── AccountStatus filter ──────────────────────────────────────────────
        if (request.AccountStatus.HasValue)
            query = query.Where(u => u.AccountStatus == request.AccountStatus.Value);

        // ── IsVerified filter (Lawyer-only, applied via OfType) ───────────────
        if (request.IsVerified.HasValue)
        {
            var verifiedFlag = request.IsVerified.Value;
            query = query.Where(u => u is Lawyer && ((Lawyer)(object)u).IsVerified == verifiedFlag);
        }

        // ── Search filter ─────────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(normalizedSearch))
            query = query.Where(u =>
                u.FullName.ToLower().Contains(normalizedSearch) ||
                u.Email.ToLower().Contains(normalizedSearch));

        // ── Cursor filter (page after last seen item) ─────────────────────────
        if (request.CursorDate.HasValue && request.Cursor.HasValue)
        {
            var cd = request.CursorDate.Value;
            var cId = request.Cursor.Value;
            query = query.Where(u =>
                u.CreatedAt < cd ||
                (u.CreatedAt == cd && u.Id < cId));
        }

        // ── Sort + fetch pageSize + 1 to detect HasMore ───────────────────────
        var raw = await query
            .OrderByDescending(u => u.CreatedAt)
            .ThenByDescending(u => u.Id)
            .Take(pageSize + 1)
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                ProfileImage = u.AvatarUrl,
                u.Email,
                u.CreatedAt,
                u.AccountStatus,
                IsLawyer = u is Lawyer,
                IsVerified = u is Lawyer ? ((Lawyer)(object)u).IsVerified : (bool?)null
            })
            .ToListAsync(cancellationToken);

        var hasMore = raw.Count > pageSize;
        if (hasMore)
            raw.RemoveAt(raw.Count - 1);

        var items = raw.Select(u => new UserListItemDto
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            ProfileImage = u.ProfileImage,
            Email = u.Email,
            UserRole = u.IsLawyer ? UserRole.Lawyer : UserRole.Client,
            RegisteredAt = u.CreatedAt,
            AccountStatus = u.AccountStatus,
            IsVerified = u.IsVerified
        }).ToList();

        int? nextCursor = null;
        DateTime? nextCursorDate = null;

        if (hasMore && items.Count > 0)
        {
            var last = items.Last();
            nextCursor = last.Id;
            nextCursorDate = last.RegisteredAt;
        }

        var result = new CursorPagedResult<UserListItemDto>
        {
            Items = items,
            NextCursor = nextCursor,
            NextCursorDate = nextCursorDate,
            HasMore = hasMore
        };

        return Result<CursorPagedResult<UserListItemDto>>.Success(result);
    }
}
