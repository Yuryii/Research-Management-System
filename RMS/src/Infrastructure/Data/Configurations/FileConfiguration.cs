using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RMS.Domain.Entities.Models;

namespace RMS.Infrastructure.Data.Configurations;

public class FileConfiguration : IEntityTypeConfiguration<RMS.Domain.Entities.Models.File>
{
    public void Configure(EntityTypeBuilder<RMS.Domain.Entities.Models.File> builder)
    {
        builder.Property(f => f.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(f => f.Path)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasMany(f => f.ApplicationFiles)
            .WithOne(af => af.File)
            .HasForeignKey(af => af.FileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
