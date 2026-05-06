using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RMS.Domain.Entities.Models;

namespace RMS.Infrastructure.Data.Configurations;

public class ApplicationFileConfiguration : IEntityTypeConfiguration<ApplicationFile>
{
    public void Configure(EntityTypeBuilder<ApplicationFile> builder)
    {
        builder.HasKey(af => new { af.ApplicationId, af.FileId });
        builder.HasOne(af => af.Step)
            .WithMany(s => s.ApplicationFiles)
            .HasForeignKey(af => af.StepId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
