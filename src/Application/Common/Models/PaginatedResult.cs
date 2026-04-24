namespace RMS.Application.Common.Models;

public record PaginatedResult<T>(IReadOnlyList<T> Items, int TotalCount, int PageNumber, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
