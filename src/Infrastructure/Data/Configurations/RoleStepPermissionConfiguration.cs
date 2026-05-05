using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RMS.Domain.Entities;
using RMS.Infrastructure.Identity;

namespace RMS.Infrastructure.Data.Configurations;

public class RoleStepPermissionConfiguration : IEntityTypeConfiguration<RoleStepPermission>
{
    public void Configure(EntityTypeBuilder<RoleStepPermission> builder)
    {
        builder.HasKey(rsp => new { rsp.RoleId, rsp.StepId });

        builder.HasOne<ApplicationRole>()
            .WithMany(role => role.RoleStepPermissions)
            .HasForeignKey(rsp => rsp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rsp => rsp.Step)
            .WithMany(s => s.RoleStepPermissions)
            .HasForeignKey(rsp => rsp.StepId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
