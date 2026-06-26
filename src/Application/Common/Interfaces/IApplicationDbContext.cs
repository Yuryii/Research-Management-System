using RMS.Domain.Entities;
using RMS.Domain.Entities.Models;
using DomainFile = RMS.Domain.Entities.Models.File;
using DomainApplication = RMS.Domain.Entities.Models.Application;

namespace RMS.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<DomainApplication> Applications { get; }
    DbSet<Step> Steps { get; }
    DbSet<StepDetail> StepDetails { get; }
    DbSet<DomainFile> Files { get; }
    DbSet<ApplicationFile> ApplicationFiles { get; }
    DbSet<ApplicationReturn> ApplicationReturns { get; }
    DbSet<ApplicationReturnFile> ApplicationReturnFiles { get; }
    DbSet<RoleStepPermission> RoleStepPermissions { get; }
    DbSet<AcademicDegree> AcademicDegrees { get; }
    DbSet<ResearchHour> ResearchHours { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<NotificationRecipient> NotificationRecipients { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
