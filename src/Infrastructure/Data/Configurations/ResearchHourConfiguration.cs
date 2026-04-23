using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RMS.Domain.Entities.Models;

namespace RMS.Infrastructure.Data.Configurations;

public class ResearchHourConfiguration : IEntityTypeConfiguration<ResearchHour>
{
    public void Configure(EntityTypeBuilder<ResearchHour> builder)
    {
        builder.Property(rh => rh.Hours)
            .HasMaxLength(10)
            .IsRequired();
    }
}
