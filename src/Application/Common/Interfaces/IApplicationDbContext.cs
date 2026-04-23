using RMS.Domain.Entities;
using RMS.Domain.Entities.Models;

namespace RMS.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    DbSet<RMS.Domain.Entities.Models.Application> Applications { get; }
    DbSet<Step> Steps { get; }
    DbSet<StepDetail> StepDetails { get; }
    DbSet<Domain.Entities.Models.File> Files { get; }
    DbSet<ApplicationFile> ApplicationFiles { get; }
    DbSet<RoleStepPermission> RoleStepPermissions { get; }
    DbSet<AcademicDegree> AcademicDegrees { get; }
    DbSet<ResearchHour> ResearchHours { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
