using System;
using System.Collections.Generic;
using System.Text;
using RMS.Domain.Entities.Models;

namespace RMS.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string? RecipientId { get; set; }
    public IList<NotificationFile> NotificationFiles { get; set; } = new List<NotificationFile>();
}
