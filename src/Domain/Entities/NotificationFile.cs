namespace RMS.Domain.Entities.Models;

public partial class NotificationFile
{
    public Guid NotificationId { get; set; }
    public Notification Notification { get; set; } = null!;
    public Guid FileId { get; set; }
    public File File { get; set; } = null!;
}
