using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Entities;
using RMS.Domain.Entities.Models;
using RMS.Infrastructure.Identity;
using DomainApplication = RMS.Domain.Entities.Models.Application;
using DomainFile = RMS.Domain.Entities.Models.File;
namespace RMS.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<TodoList> TodoLists => Set<TodoList>();

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public DbSet<Step> Steps => Set<Step>();
    public DbSet<DomainApplication> Applications => Set<DomainApplication>();
    public DbSet<DomainFile> Files => Set<DomainFile>();
    public DbSet<ApplicationFile> ApplicationFiles => Set<ApplicationFile>();
    public DbSet<RoleStepPermission> RoleStepPermissions => Set<RoleStepPermission>();
    public DbSet<StepDetail> StepDetails => Set<StepDetail>();
    public DbSet<AcademicDegree> AcademicDegrees => Set<AcademicDegree>();
    public DbSet<ResearchHour> ResearchHours => Set<ResearchHour>();
    public DbSet<ApplicationReturn> ApplicationReturns => Set<ApplicationReturn>();
    public DbSet<ApplicationReturnFile> ApplicationReturnFiles => Set<ApplicationReturnFile>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationRecipient> NotificationRecipients => Set<NotificationRecipient>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
