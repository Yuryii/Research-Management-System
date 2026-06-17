using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RMS.Domain.Entities;

namespace RMS.Infrastructure.Data.Configurations;

public class NotificationRecipientConfiguration : IEntityTypeConfiguration<NotificationRecipient>
{
    public void Configure(EntityTypeBuilder<NotificationRecipient> builder)
    {
        builder.HasKey(x => new { x.NotificationId, x.UserId });

        builder.Property(x => x.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasOne(x => x.Notification)
               .WithMany(n => n.Recipients)
               .HasForeignKey(x => x.NotificationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.IsRead });
    }
}
