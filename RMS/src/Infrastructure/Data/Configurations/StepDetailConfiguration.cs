using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RMS.Domain.Entities.Models;

namespace RMS.Infrastructure.Data.Configurations;

public class StepDetailConfiguration : IEntityTypeConfiguration<StepDetail>
{
    public void Configure(EntityTypeBuilder<StepDetail> builder)
    {
        builder.Property(sd => sd.NameUserScreen)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(sd => sd.NameAdminScreen)
            .HasMaxLength(200)
            .IsRequired();
    }
}
