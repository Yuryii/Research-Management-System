using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RMS.Application.Common.Interfaces;

public interface IApplicationFileService
{
    Task AddFilesToApplicationAsync(
        Guid applicationId,
        Guid stepId,
        IFormFileCollection files,
        CancellationToken cancellationToken = default);
}
