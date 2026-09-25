namespace AlMostashar.Application.Common.Models;

/// <summary>
/// Generic cursor-based pagination wrapper.
/// The cursor is the Id of the last item in the current page.
/// </summary>
public class CursorPagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int? NextCursor { get; set; }

    /// <summary>
    /// CreatedAt/date of the last item; pass back alongside NextCursor
    /// so the server can filter by (date, id) for stable tie-breaking.
    /// </summary>
    public DateTime? NextCursorDate { get; set; }

    public bool HasMore { get; set; }
}
