using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RMS.Domain.Entities.Models;

namespace RMS.Infrastructure.Data.Configurations;

public class AcademicDegreeConfiguration : IEntityTypeConfiguration<AcademicDegree>
{
    public void Configure(EntityTypeBuilder<AcademicDegree> builder)
    {
        builder.Property(ad => ad.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(ad => ad.Description)
            .HasMaxLength(1000)
            .IsRequired();
    }
}
