using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Entities;
using RMS.Domain.Entities.Models;
using RMS.Infrastructure.Identity;

namespace RMS.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<TodoList> TodoLists => Set<TodoList>();

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public DbSet<Step> Steps => Set<Step>();
    public DbSet<RMS.Domain.Entities.Models.Application> Applications => Set<RMS.Domain.Entities.Models.Application>();
    public DbSet<RMS.Domain.Entities.Models.File> Files => Set<RMS.Domain.Entities.Models.File>();
    public DbSet<ApplicationFile> ApplicationFiles => Set<ApplicationFile>();
    public DbSet<RoleStepPermission> RoleStepPermissions => Set<RoleStepPermission>();
    public DbSet<StepDetail> StepDetails => Set<StepDetail>();
    public DbSet<AcademicDegree> AcademicDegrees => Set<AcademicDegree>();
    public DbSet<ResearchHour> ResearchHours => Set<ResearchHour>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
