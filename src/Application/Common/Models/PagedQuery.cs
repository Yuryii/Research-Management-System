using RMS.Application.Common.Interfaces;

namespace RMS.Application.Common.Models;

public abstract record PagedQuery<T> : IPagedQuery, MediatR.IRequest<PaginatedResult<T>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
