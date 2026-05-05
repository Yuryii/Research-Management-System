using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RMS.Domain.Entities;
using RMS.Infrastructure.Identity;

namespace RMS.Infrastructure.Data.Configurations;

public class RoleStepPermissionConfiguration : IEntityTypeConfiguration<RoleStepPermission>
{
    public void Configure(EntityTypeBuilder<RoleStepPermission> builder)
    {
        builder.HasKey(rsp => new { rsp.RoleId, rsp.StepDetailId });

        builder.HasOne<ApplicationRole>()
            .WithMany(role => role.RoleStepPermissions)
            .HasForeignKey(rsp => rsp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rsp => rsp.StepDetail)
            .WithMany(sd => sd.RoleStepPermissions)
            .HasForeignKey(rsp => rsp.StepDetailId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
