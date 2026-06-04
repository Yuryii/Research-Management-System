using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RMS.Domain.Entities.Models;

namespace RMS.Infrastructure.Data.Configurations;

public class ApplicationReturnFileConfiguration : IEntityTypeConfiguration<ApplicationReturnFile>
{
    public void Configure(EntityTypeBuilder<ApplicationReturnFile> builder)
    {
        builder.HasKey(nf => new { nf.ApplicationReturnId, nf.FileId });

        builder.HasOne(nf => nf.ApplicationReturn)
            .WithMany(n => n.ApplicationReturnFiles)
            .HasForeignKey(nf => nf.ApplicationReturnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(nf => nf.File)
            .WithMany(f => f.ApplicationReturnFiles)
            .HasForeignKey(nf => nf.FileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
