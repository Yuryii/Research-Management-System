using System;
using System.Collections.Generic;
using System.Text;

namespace RMS.Application.Application.Commands.ForwardNextToStep;

public record ForwardNextToStepCommand : IRequest<Guid>
{
    public Guid ApplicationId { get; set; }
}
