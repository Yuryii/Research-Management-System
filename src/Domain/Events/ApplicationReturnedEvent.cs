namespace RMS.Domain.Events;

public class ApplicationReturnedEvent : BaseEvent
{
    public Guid ApplicationId { get; init; }
    public required string ApplicationCode { get; init; }
    public required string RecipientId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
}
