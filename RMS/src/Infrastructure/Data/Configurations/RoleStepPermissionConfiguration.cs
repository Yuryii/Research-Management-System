using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RMS.Domain.Entities;

namespace RMS.Infrastructure.Data.Configurations;

public class RoleStepPermissionConfiguration : IEntityTypeConfiguration<RoleStepPermission>
{
    public void Configure(EntityTypeBuilder<RoleStepPermission> builder)
    {
        builder.HasKey(rsp => new { rsp.RoleId, rsp.StepId });
    }
}
