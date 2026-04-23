namespace RMS.Application.Common.Interfaces;

public interface IStepResolver
{
    /// <summary>
    /// Resolves the default <c>StepDetail</c> identifier used when no explicit step detail is provided.
    /// </summary>
    /// <param name="cancellationToken">A token used to observe cancellation requests.</param>
    /// <returns>The identifier of the first available <c>StepDetail</c> ordered by <c>StepId</c> and <c>Id</c>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no <c>StepDetail</c> records exist.</exception>
    Task<Guid> ResolveAsync(CancellationToken cancellationToken);
}
