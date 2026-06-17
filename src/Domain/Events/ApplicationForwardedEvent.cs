namespace RMS.Domain.Events;

public class ApplicationForwardedEvent : BaseEvent
{
    public Guid ApplicationId { get; init; }
    public required string ApplicationCode { get; init; }
    public required string FromStepName { get; init; }
    public required string ToStepName { get; init; }

    // The step the application was forwarded TO. Carried explicitly in the event
    // so notification handlers don't depend on (or accidentally race with) the
    // application's mutated StepDetailId at the time of dispatch.
    public Guid NextStepId { get; init; }
    public Guid FromStepId { get; init; }
}
