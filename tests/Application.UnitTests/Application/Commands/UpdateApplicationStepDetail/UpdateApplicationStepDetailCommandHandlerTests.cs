using System.Collections.Generic;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RMS.Application.Application.Commands.UpdateApplicationStepDetail;
using RMS.Application.Common.Exceptions;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Enums;
using RMS.Domain.Entities;
using RMS.Domain.Entities.Models;
using RMS.Infrastructure.Data;
using Shouldly;
using DomainApplication = RMS.Domain.Entities.Models.Application;
using NotFoundException = Ardalis.GuardClauses.NotFoundException;

namespace RMS.Application.UnitTests.Application.Commands;

public class UpdateApplicationStepDetailCommandHandlerTests : IDisposable
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<IUser> _userMock = null!;
    private Mock<IIdentityService> _identityServiceMock = null!;

    private static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [SetUp]
    public void SetUp()
    {
        _dbContext = CreateInMemoryContext();
        _userMock = new Mock<IUser>();
        _identityServiceMock = new Mock<IIdentityService>();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }

    private UpdateApplicationStepDetailCommandHandler CreateHandler()
        => new(_dbContext, _userMock.Object, _identityServiceMock.Object);

    private Step AddStep()
    {
        var step = new Step
        {
            Id = Guid.NewGuid(),
            Name = "Test Step",
            Order = 1,
        };
        _dbContext.Steps.Add(step);
        _dbContext.SaveChanges();
        return step;
    }

    private StepDetail AddStepDetail(Step step, bool isReturnStep = false)
    {
        var stepDetail = new StepDetail
        {
            Id = Guid.NewGuid(),
            Name = isReturnStep ? "Return Step" : "Normal Step",
            Order = 1,
            StepId = step.Id,
            Step = step,
            IsReturnStep = isReturnStep,
            IsCaculateScoreStep = false
        };
        _dbContext.StepDetails.Add(stepDetail);
        _dbContext.SaveChanges();
        return stepDetail;
    }

    private DomainApplication AddApplication(StepDetail stepDetail)
    {
        var app = new DomainApplication
        {
            Id = Guid.NewGuid(),
            Code = "APP-001",
            Title = "Test Application",
            Description = "Test Description",
            Status = ApplicationStatus.Submitted,
            StepDetailId = stepDetail.Id
        };
        _dbContext.Applications.Add(app);
        _dbContext.SaveChanges();
        return app;
    }

    private void AddRoleStepPermission(Step step, string roleId)
    {
        var permission = new RoleStepPermission
        {
            StepId = step.Id,
            RoleId = roleId
        };
        _dbContext.RoleStepPermissions.Add(permission);
        _dbContext.SaveChanges();
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenApplicationDoesNotExist()
    {
        var step = AddStep();
        var stepDetail = AddStepDetail(step);
        var command = new UpdateApplicationStepDetailCommand
        {
            ApplicationId = Guid.NewGuid(),
            StepDetailId = stepDetail.Id
        };
        var handler = CreateHandler();

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenApplicationIsInReturnStep()
    {
        var step = AddStep();
        var returnStepDetail = AddStepDetail(step, isReturnStep: true);
        var app = AddApplication(returnStepDetail);
        var newStepDetail = AddStepDetail(step);
        var command = new UpdateApplicationStepDetailCommand
        {
            ApplicationId = app.Id,
            StepDetailId = newStepDetail.Id
        };
        var handler = CreateHandler();

        await Should.ThrowAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenStepDetailDoesNotExist()
    {
        var step = AddStep();
        var stepDetail = AddStepDetail(step);
        var app = AddApplication(stepDetail);
        var command = new UpdateApplicationStepDetailCommand
        {
            ApplicationId = app.Id,
            StepDetailId = Guid.NewGuid()
        };
        var handler = CreateHandler();

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ShouldThrowForbiddenAccessException_WhenUserHasNoPermission()
    {
        var step = AddStep();
        var currentStepDetail = AddStepDetail(step);
        var newStepDetail = AddStepDetail(step);
        var app = AddApplication(currentStepDetail);

        _userMock.Setup(u => u.Roles).Returns(new List<string> { "UnauthorizedRole" });
        _identityServiceMock
            .Setup(s => s.GetRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "unauthorized-role-id" });

        var command = new UpdateApplicationStepDetailCommand
        {
            ApplicationId = app.Id,
            StepDetailId = newStepDetail.Id
        };
        var handler = CreateHandler();

        await Should.ThrowAsync<ForbiddenAccessException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ShouldUpdateStepDetailId_WhenUserHasPermission()
    {
        var step = AddStep();
        var currentStepDetail = AddStepDetail(step);
        var newStepDetail = AddStepDetail(step);
        var app = AddApplication(currentStepDetail);
        var roleId = "teacher-role-id";

        AddRoleStepPermission(step, roleId);

        _userMock.Setup(u => u.Roles).Returns(new List<string> { "Teacher" });
        _identityServiceMock
            .Setup(s => s.GetRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { roleId });

        var command = new UpdateApplicationStepDetailCommand
        {
            ApplicationId = app.Id,
            StepDetailId = newStepDetail.Id
        };
        var handler = CreateHandler();

        await handler.Handle(command, CancellationToken.None);

        var updatedApp = await _dbContext.Applications.FindAsync(app.Id);
        updatedApp!.StepDetailId.ShouldBe(newStepDetail.Id);
    }
}
