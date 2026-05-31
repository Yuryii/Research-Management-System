using Microsoft.Extensions.Options;
using RMS.Application.Common.Interfaces;
using RMS.Application.Common.Options;
using RMS.Domain.Constants;

namespace RMS.Application.Steps.Queries;

public record GetDefaultStepIdForUserQuery : IRequest<Guid>;

public class GetDefaultStepIdForUserQueryHandler : IRequestHandler<GetDefaultStepIdForUserQuery, Guid>
{
    private readonly IUser _user;
    private readonly IOptions<DefaultStepIdsOptions> _stepIdsOptions;

    public GetDefaultStepIdForUserQueryHandler(IUser user, IOptions<DefaultStepIdsOptions> stepIdsOptions)
    {
        _user = user;
        _stepIdsOptions = stepIdsOptions;
    }

    public Task<Guid> Handle(GetDefaultStepIdForUserQuery request, CancellationToken cancellationToken)
    {
        var roles = _user.Roles ?? [];
        var stepIds = _stepIdsOptions.Value;

        if (roles.Contains(Roles.Teacher))
            return Task.FromResult(stepIds.TeacherStepId);
        if (roles.Contains(Roles.Dvqltt))
            return Task.FromResult(stepIds.DvqlttStepId);
        if (roles.Contains(Roles.Tttv))
            return Task.FromResult(stepIds.TttvStepId);
        if (roles.Contains(Roles.KhcnHtqt))
            return Task.FromResult(stepIds.KhcnHtqtStepId);
        if (roles.Contains(Roles.Administrator))
        {
            if (stepIds.DvqlttStepId != Guid.Empty)
                return Task.FromResult(stepIds.DvqlttStepId);
            if (stepIds.TeacherStepId != Guid.Empty)
                return Task.FromResult(stepIds.TeacherStepId);
        }

        return Task.FromResult(Guid.Empty);
    }
}
