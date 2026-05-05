using RMS.Application.Application.Commands.UpdateApplicationStepDetail;
using RMS.Application.Common.Exceptions;
using RMS.Domain.Constants;
using RMS.Domain.Entities;
using RMS.Domain.Entities.Models;
using RMS.Domain.Enums;
using RMS.Infrastructure.Data;
using RMS.Infrastructure.Identity;
using DomainApplication = RMS.Domain.Entities.Models.Application;

namespace RMS.Application.FunctionalTests.Application.Commands;

public class UpdateApplicationStepDetailTests : TestBase
{
    [Test]
    public async Task ShouldRequireValidApplicationId()
    {
        await TestApp.RunAsAdministratorAsync();

        var stepDetail = await CreateStepDetailAsync(Roles.Administrator);
        var command = new UpdateApplicationStepDetailCommand
        {
            ApplicationId = Guid.NewGuid(),
            StepDetailId = stepDetail.Id!.Value
        };

        await Should.ThrowAsync<NotFoundException>(() => TestApp.SendAsync(command));
    }

    [Test]
    public async Task ShouldRequireValidStepDetailId()
    {
        await TestApp.RunAsAdministratorAsync();

        var application = await CreateApplicationAsync();
        var command = new UpdateApplicationStepDetailCommand
        {
            ApplicationId = application.Id!.Value,
            StepDetailId = Guid.NewGuid()
        };

        await Should.ThrowAsync<NotFoundException>(() => TestApp.SendAsync(command));
    }

    [Test]
    public async Task ShouldUpdateApplicationStepDetailWhenRoleHasPermissionForTargetStep()
    {
        var userId = await TestApp.RunAsUserAsync("dvqltt-update-step-detail@local", "Testing1234!", [Roles.Dvqltt]);

        var stepDetail = await CreateStepDetailAsync(Roles.Dvqltt);
        var application = await CreateApplicationAsync();

        var command = new UpdateApplicationStepDetailCommand
        {
            ApplicationId = application.Id!.Value,
            StepDetailId = stepDetail.Id!.Value
        };

        await TestApp.SendAsync(command);

        var updatedApplication = await TestApp.FindAsync<DomainApplication>(application.Id!.Value);

        updatedApplication.ShouldNotBeNull();
        updatedApplication!.StepDetailId.ShouldBe(command.StepDetailId);
        updatedApplication.LastModifiedBy.ShouldBe(userId);
    }

    [Test]
    public async Task ShouldDenyUpdateApplicationStepDetailWhenRoleHasNoPermissionForTargetStep()
    {
        await TestApp.RunAsUserAsync("tttv-update-step-detail@local", "Testing1234!", [Roles.Tttv]);

        var stepDetail = await CreateStepDetailAsync(Roles.Dvqltt);
        var application = await CreateApplicationAsync();

        var command = new UpdateApplicationStepDetailCommand
        {
            ApplicationId = application.Id!.Value,
            StepDetailId = stepDetail.Id!.Value
        };

        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(command));

        var unchangedApplication = await TestApp.FindAsync<DomainApplication>(application.Id!.Value);

        unchangedApplication.ShouldNotBeNull();
        unchangedApplication!.StepDetailId.ShouldBe(application.StepDetailId);
    }

    private static async Task<StepDetail> CreateStepDetailAsync(string permittedRoleName)
    {
        var step = new Step
        {
            Id = Guid.NewGuid(),
            Name = $"{permittedRoleName} step",
            ShortName = permittedRoleName,
            Order = 1,
            StepDetails =
            {
                new StepDetail
                {
                    Id = Guid.NewGuid(),
                    Name = $"{permittedRoleName} step detail",
                    Order = 1
                }
            }
        };

        await TestApp.AddAsync(step);

        await TestApp.ExecuteDbContextAsync(async context =>
        {
            var role = await context.Roles.SingleAsync(role => role.Name == permittedRoleName);
            context.RoleStepPermissions.Add(new RoleStepPermission
            {
                RoleId = role.Id,
                StepId = step.Id!.Value
            });

            await context.SaveChangesAsync();
        });

        return step.StepDetails.Single();
    }

    private static async Task<DomainApplication> CreateApplicationAsync()
    {
        var initialStepDetail = await CreateStepDetailAsync(Roles.Administrator);
        var application = new DomainApplication
        {
            Id = Guid.NewGuid(),
            Code = $"APP-{Guid.NewGuid():N}"[..20],
            Title = "Application for step detail update",
            Description = "Application created by functional tests.",
            Status = ApplicationStatus.Draft,
            StepDetailId = initialStepDetail.Id!.Value
        };

        await TestApp.AddAsync(application);

        return application;
    }
}
