using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RMS.Domain.Entities.Models;
using RMS.Domain.Enums;

namespace RMS.Infrastructure.Data.Configurations;

public class ApplicationConfiguration : IEntityTypeConfiguration<RMS.Domain.Entities.Models.Application>
{
    public void Configure(EntityTypeBuilder<RMS.Domain.Entities.Models.Application> builder)
    {
        builder.Property(a => a.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasOne(a => a.StepDetail)
            .WithMany(sd => sd.Applications)
            .HasForeignKey(a => a.StepDetailId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.ApplicationFiles)
            .WithOne(af => af.Application)
            .HasForeignKey(af => af.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
