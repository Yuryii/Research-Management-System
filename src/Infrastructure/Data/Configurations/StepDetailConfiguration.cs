using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RMS.Domain.Entities.Models;

namespace RMS.Infrastructure.Data.Configurations;

public class StepDetailConfiguration : IEntityTypeConfiguration<StepDetail>
{
    public void Configure(EntityTypeBuilder<StepDetail> builder)
    {
        builder.Property(sd => sd.Name)
            .HasMaxLength(200)
            .IsRequired();
        builder.HasOne(sd => sd.NextStepDetail)
    .WithMany() 
    .HasForeignKey(sd => sd.NextStepDetailId)
    .OnDelete(DeleteBehavior.Restrict);
    }
}
