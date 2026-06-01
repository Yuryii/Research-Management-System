using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using RMS.Application.Common.Interfaces;
using RMS.Application.Common.Models;

namespace RMS.Application.Common.Extensions;

public static class PaginationExtensions
{
    public static async Task<PaginatedResult<TResult>> ToPaginatedResultAsync<T, TResult, TKey>(
        this IQueryable<T> query,
        IPagedQuery request,
        Expression<Func<T, TKey>> orderBy,
        AutoMapper.IConfigurationProvider configurationProvider,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(orderBy)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .AsNoTracking()
            .ProjectTo<TResult>(configurationProvider)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<TResult>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
