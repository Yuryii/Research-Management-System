using System;
using System.Collections.Generic;
using System.Text;
using RMS.Application.Application.Dtos;
using RMS.Application.Common.Models;

namespace RMS.Application.Application.Queries.GetApplications;

public record GetApplicationsQuery : IRequest<PaginatedResult<ApplicationDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public Guid? StepId { get; set; }
}
