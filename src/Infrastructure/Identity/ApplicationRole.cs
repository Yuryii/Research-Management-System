using System.Collections;
using Microsoft.AspNetCore.Identity;
using RMS.Domain.Entities;

namespace RMS.Infrastructure.Identity;

public class ApplicationRole : IdentityRole
{
    public IList<RoleStepPermission> RoleStepPermissions { get; set; } = new List<RoleStepPermission>();
}
