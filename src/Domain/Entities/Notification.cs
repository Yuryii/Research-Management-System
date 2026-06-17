using RMS.Domain.Common;

namespace RMS.Domain.Entities;

public class Notification : BaseAuditableEntity<Guid>
{
    public required string Title { get; set; }
    public required string Body { get; set; }
    public required NotificationType Type { get; set; }
    public Guid? RelatedApplicationId { get; set; }

    public IList<NotificationRecipient> Recipients { get; set; } = new List<NotificationRecipient>();
}

public enum NotificationType
{
    ApplicationReturned = 1,
    ApplicationForwarded = 2,
    ApplicationApproved = 3,
    SystemAnnouncement = 99,
}
