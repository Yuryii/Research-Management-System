namespace RMS.Domain.Entities;

public class NotificationRecipient
{
    public required Guid NotificationId { get; set; }
    public required string UserId { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }

    public Notification Notification { get; set; } = null!;
}
