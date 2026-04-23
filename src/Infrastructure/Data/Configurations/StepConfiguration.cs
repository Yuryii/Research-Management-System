using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RMS.Domain.Entities.Models;

namespace RMS.Infrastructure.Data.Configurations;

public class StepConfiguration : IEntityTypeConfiguration<Step>
{
    public void Configure(EntityTypeBuilder<Step> builder)
    {
        builder.Property(s => s.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasMany(s => s.StepDetails)
            .WithOne(sd => sd.Step)
            .HasForeignKey(sd => sd.StepId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.RoleStepPermission)
            .WithOne(rsp => rsp.Step)
            .HasForeignKey(rsp => rsp.StepId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
