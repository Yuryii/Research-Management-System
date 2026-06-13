using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RMS.Domain.Entities;

namespace RMS.Infrastructure.Data.Configurations;

public class ApplicationReturnConfiguration : IEntityTypeConfiguration<ApplicationReturn>
{
    public void Configure(EntityTypeBuilder<ApplicationReturn> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Application)
            .WithMany()
            .HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
