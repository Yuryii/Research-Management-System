using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RMS.Domain.Entities.Models;

namespace RMS.Infrastructure.Data.Configurations;

public class NotificationFileConfiguration : IEntityTypeConfiguration<NotificationFile>
{
    public void Configure(EntityTypeBuilder<NotificationFile> builder)
    {
        builder.HasKey(nf => new { nf.NotificationId, nf.FileId });

        builder.HasOne(nf => nf.Notification)
            .WithMany(n => n.NotificationFiles)
            .HasForeignKey(nf => nf.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(nf => nf.File)
            .WithMany(f => f.NotificationFiles)
            .HasForeignKey(nf => nf.FileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
