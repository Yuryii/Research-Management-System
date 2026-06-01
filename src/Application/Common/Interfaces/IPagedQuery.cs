namespace RMS.Application.Common.Interfaces;

public interface IPagedQuery
{
    int PageNumber { get; }
    int PageSize { get; }
}
