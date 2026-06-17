using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RMS.Application.Common.EventHandlers;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Entities;
using RMS.Domain.Entities.Models;
using RMS.Domain.Events;
using RMS.Infrastructure.Data;
using Shouldly;

namespace RMS.Application.UnitTests.Common.EventHandlers;

public class ApplicationForwardedNotificationHandlerTests : IDisposable
{
    private ApplicationDbContext _dbContext = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);
    }

    [TearDown]
    public void TearDown() => _dbContext.Dispose();
    public void Dispose() => _dbContext?.Dispose();

    [Test]
    public async Task Handle_ShouldCreateNotification_WithRecipientsForAllUsersInNextStepRoles()
    {
        var nextStep = new Step { Id = Guid.NewGuid(), Name = "Next", Order = 2, NextStepId = null };
        var nextStepDetail = new StepDetail { Id = Guid.NewGuid(), StepId = nextStep.Id, Name = "D1", Order = 1, IsReturnStep = false };
        var roleId = "role-x";
        _dbContext.Steps.Add(nextStep);
        _dbContext.StepDetails.Add(nextStepDetail);
        _dbContext.RoleStepPermissions.Add(new RoleStepPermission { RoleId = roleId, StepId = nextStep.Id });

        var app = new Domain.Entities.Models.Application
        {
            Id = Guid.NewGuid(),
            Code = "APP-1",
            Title = "t",
            Description = "d",
            StepDetailId = nextStepDetail.Id,
        };
        _dbContext.Applications.Add(app);
        await _dbContext.SaveChangesAsync();

        var identityMock = new Mock<IIdentityService>();
        identityMock
            .Setup(s => s.GetUserIdsInRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "u1", "u2" });

        var handler = new ApplicationForwardedNotificationHandler(_dbContext, identityMock.Object);

        await handler.Handle(new ApplicationForwardedEvent
        {
            ApplicationId = app.Id,
            ApplicationCode = "APP-1",
            FromStepName = "A",
            ToStepName = "B",
            FromStepId = Guid.NewGuid(),
            NextStepId = nextStep.Id,
        }, CancellationToken.None);

        var notifications = await _dbContext.Notifications.ToListAsync();
        notifications.Count.ShouldBe(1);
        notifications[0].Type.ShouldBe(NotificationType.ApplicationForwarded);
        notifications[0].RelatedApplicationId.ShouldBe(app.Id);
        notifications[0].Body.ShouldContain("APP-1");
        notifications[0].Body.ShouldContain("A");
        notifications[0].Body.ShouldContain("B");

        var recipients = await _dbContext.NotificationRecipients.ToListAsync();
        recipients.Count.ShouldBe(2);
        recipients.ShouldAllBe(r => !r.IsRead);
        recipients.ShouldAllBe(r => r.NotificationId == notifications[0].Id);
        recipients.Select(r => r.UserId).ShouldContain("u1");
        recipients.Select(r => r.UserId).ShouldContain("u2");

        identityMock.Verify(
            s => s.GetUserIdsInRoleIdsAsync(
                It.Is<IEnumerable<string>>(ids => ids.Contains(roleId)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_ShouldSkip_WhenNoUsersInRoles()
    {
        var nextStep = new Step { Id = Guid.NewGuid(), Name = "Next", Order = 2, NextStepId = null };
        var nextStepDetail = new StepDetail { Id = Guid.NewGuid(), StepId = nextStep.Id, Name = "D1", Order = 1, IsReturnStep = false };
        var roleId = "role-x";
        _dbContext.Steps.Add(nextStep);
        _dbContext.StepDetails.Add(nextStepDetail);
        _dbContext.RoleStepPermissions.Add(new RoleStepPermission { RoleId = roleId, StepId = nextStep.Id });

        var app = new Domain.Entities.Models.Application
        {
            Id = Guid.NewGuid(),
            Code = "APP-1",
            Title = "t",
            Description = "d",
            StepDetailId = nextStepDetail.Id,
        };
        _dbContext.Applications.Add(app);
        await _dbContext.SaveChangesAsync();

        var identityMock = new Mock<IIdentityService>();
        identityMock
            .Setup(s => s.GetUserIdsInRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        var handler = new ApplicationForwardedNotificationHandler(_dbContext, identityMock.Object);

        await handler.Handle(new ApplicationForwardedEvent
        {
            ApplicationId = app.Id,
            ApplicationCode = "APP-1",
            FromStepName = "A",
            ToStepName = "B",
            FromStepId = Guid.NewGuid(),
            NextStepId = nextStep.Id,
        }, CancellationToken.None);

        var notifications = await _dbContext.Notifications.ToListAsync();
        notifications.Count.ShouldBe(0);
    }

    [Test]
    public async Task Handle_ShouldSkip_WhenNoRolesForNextStep()
    {
        var nextStep = new Step { Id = Guid.NewGuid(), Name = "Next", Order = 2, NextStepId = null };
        var nextStepDetail = new StepDetail { Id = Guid.NewGuid(), StepId = nextStep.Id, Name = "D1", Order = 1, IsReturnStep = false };
        _dbContext.Steps.Add(nextStep);
        _dbContext.StepDetails.Add(nextStepDetail);
        // Note: no RoleStepPermission added for this step

        var app = new Domain.Entities.Models.Application
        {
            Id = Guid.NewGuid(),
            Code = "APP-1",
            Title = "t",
            Description = "d",
            StepDetailId = nextStepDetail.Id,
        };
        _dbContext.Applications.Add(app);
        await _dbContext.SaveChangesAsync();

        var identityMock = new Mock<IIdentityService>();
        identityMock
            .Setup(s => s.GetUserIdsInRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "u1" });

        var handler = new ApplicationForwardedNotificationHandler(_dbContext, identityMock.Object);

        await handler.Handle(new ApplicationForwardedEvent
        {
            ApplicationId = app.Id,
            ApplicationCode = "APP-1",
            FromStepName = "A",
            ToStepName = "B",
            FromStepId = Guid.NewGuid(),
            NextStepId = nextStep.Id,
        }, CancellationToken.None);

        var notifications = await _dbContext.Notifications.ToListAsync();
        notifications.Count.ShouldBe(0);
    }

    [Test]
    public async Task Handle_ShouldNotRequireApplicationEntity()
    {
        // The handler must work without the application entity being present in the
        // DbContext, since the recipient resolution should rely solely on the
        // event payload (NextStepId), not on a fresh load of the mutated entity.
        var nextStep = new Step { Id = Guid.NewGuid(), Name = "Next", Order = 2, NextStepId = null };
        _dbContext.Steps.Add(nextStep);
        _dbContext.RoleStepPermissions.Add(new RoleStepPermission { RoleId = "role-x", StepId = nextStep.Id });
        await _dbContext.SaveChangesAsync();

        var identityMock = new Mock<IIdentityService>();
        identityMock
            .Setup(s => s.GetUserIdsInRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "u1" });

        var handler = new ApplicationForwardedNotificationHandler(_dbContext, identityMock.Object);

        // Note: no Application is added to the context. With the previous
        // implementation this would early-return at the app-load step.
        await handler.Handle(new ApplicationForwardedEvent
        {
            ApplicationId = Guid.NewGuid(),
            ApplicationCode = "APP-1",
            FromStepName = "A",
            ToStepName = "B",
            FromStepId = Guid.NewGuid(),
            NextStepId = nextStep.Id,
        }, CancellationToken.None);

        var notifications = await _dbContext.Notifications.ToListAsync();
        notifications.Count.ShouldBe(1);
        var recipients = await _dbContext.NotificationRecipients.ToListAsync();
        recipients.Count.ShouldBe(1);
        recipients[0].UserId.ShouldBe("u1");
    }

    [Test]
    public async Task Handle_ShouldNotifyUsersInNextStepRoles_NotCurrentStepRoles()
    {
        // Regression test: when the application is currently at "current step" but is
        // being forwarded to "next step", recipients must be resolved from the NEXT
        // step's role permissions, not the application's current step. The event
        // payload is the source of truth — verifying that the handler asks
        // IIdentityService for users in the *next* step's role id only.
        var currentStep = new Step { Id = Guid.NewGuid(), Name = "Current", Order = 1 };
        var nextStep = new Step { Id = Guid.NewGuid(), Name = "Next", Order = 2, NextStepId = null };

        _dbContext.Steps.AddRange(currentStep, nextStep);
        _dbContext.RoleStepPermissions.Add(new RoleStepPermission { RoleId = "role-current", StepId = currentStep.Id });
        _dbContext.RoleStepPermissions.Add(new RoleStepPermission { RoleId = "role-next", StepId = nextStep.Id });
        await _dbContext.SaveChangesAsync();

        IEnumerable<string>? capturedRoleIds = null;
        var identityMock = new Mock<IIdentityService>();
        identityMock
            .Setup(s => s.GetUserIdsInRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<string>, CancellationToken>((ids, _) => capturedRoleIds = ids)
            .ReturnsAsync(new List<string> { "u-next-1" });

        var handler = new ApplicationForwardedNotificationHandler(_dbContext, identityMock.Object);

        await handler.Handle(new ApplicationForwardedEvent
        {
            ApplicationId = Guid.NewGuid(),
            ApplicationCode = "APP-X",
            FromStepName = "Current",
            ToStepName = "Next",
            FromStepId = currentStep.Id,
            NextStepId = nextStep.Id,
        }, CancellationToken.None);

        capturedRoleIds.ShouldNotBeNull();
        capturedRoleIds!.ShouldContain("role-next");
        capturedRoleIds.ShouldNotContain("role-current");

        var recipients = await _dbContext.NotificationRecipients.ToListAsync();
        recipients.ShouldAllBe(r => r.UserId == "u-next-1");
    }
}
