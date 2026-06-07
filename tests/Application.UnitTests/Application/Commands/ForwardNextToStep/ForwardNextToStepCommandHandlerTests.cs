using System.Collections.Generic;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RMS.Application.Application.Commands.ForwardNextToStep;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Entities;
using RMS.Domain.Entities.Models;
using RMS.Infrastructure.Data;
using Shouldly;
using DomainApplication = RMS.Domain.Entities.Models.Application;
using NotFoundException = Ardalis.GuardClauses.NotFoundException;

namespace RMS.Application.UnitTests.Application.Commands;

public class ForwardNextToStepCommandHandlerTests : IDisposable
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

    private (Step step1, Step step2, StepDetail stepDetail1, StepDetail stepDetail2, string roleId) SeedBaseData()
    {
        var step1 = new Step { Id = Guid.NewGuid(), Name = "Step 1", Order = 1 };
        var step2 = new Step { Id = Guid.NewGuid(), Name = "Step 2", Order = 2, NextStepId = null };
        step1.NextStepId = step2.Id;

        var stepDetail1 = new StepDetail { Id = Guid.NewGuid(), Name = "StepDetail 1", StepId = step1.Id, Order = 1, IsReturnStep = false };
        var stepDetail2 = new StepDetail { Id = Guid.NewGuid(), Name = "StepDetail 2", StepId = step2.Id, Order = 1, IsReturnStep = false };

        var roleId = "role-1";
        var permission = new RoleStepPermission { RoleId = roleId, StepId = step1.Id, Step = step1 };

        _dbContext.Steps.AddRange(step1, step2);
        _dbContext.StepDetails.AddRange(stepDetail1, stepDetail2);
        _dbContext.RoleStepPermissions.Add(permission);
        _dbContext.SaveChanges();

        return (step1, step2, stepDetail1, stepDetail2, roleId);
    }

    private void SetupUserAndIdentityMocks(List<string> roles, string roleId)
    {
        _userMock.Setup(u => u.Roles).Returns(roles);
        _identityServiceMock
            .Setup(s => s.GetRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { roleId });
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenApplicationDoesNotExist()
    {
        var (step1, _, stepDetail1, _, roleId) = SeedBaseData();
        SetupUserAndIdentityMocks(new List<string> { roleId }, roleId);

        var handler = new ForwardNextToStepCommandHandler(_dbContext, _userMock.Object, _identityServiceMock.Object);
        var command = new ForwardNextToStepCommand { ApplicationId = Guid.NewGuid() };

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenStepDetailIsReturnStep()
    {
        var (step1, _, stepDetail1, _, roleId) = SeedBaseData();

        var stepDetailReturn = new StepDetail
        {
            Id = Guid.NewGuid(),
            Name = "Return Step",
            StepId = step1.Id,
            Order = 2,
            IsReturnStep = true
        };
        _dbContext.StepDetails.Add(stepDetailReturn);

        var app = new DomainApplication
        {
            Id = Guid.NewGuid(),
            Code = "APP-001",
            Title = "Test",
            Description = "Test",
            StepDetailId = stepDetailReturn.Id
        };
        _dbContext.Applications.Add(app);
        _dbContext.SaveChanges();

        SetupUserAndIdentityMocks(new List<string> { roleId }, roleId);

        var handler = new ForwardNextToStepCommandHandler(_dbContext, _userMock.Object, _identityServiceMock.Object);
        var command = new ForwardNextToStepCommand { ApplicationId = app.Id };

        var ex = await Should.ThrowAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        ex.Message.ShouldBe("Cannot update step detail for application in return step.");
    }

    [Test]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserRoleHasNoPermission()
    {
        var (_, _, stepDetail1, _, _) = SeedBaseData();

        var app = new DomainApplication
        {
            Id = Guid.NewGuid(),
            Code = "APP-001",
            Title = "Test",
            Description = "Test",
            StepDetailId = stepDetail1.Id
        };
        _dbContext.Applications.Add(app);
        _dbContext.SaveChanges();

        _userMock.Setup(u => u.Roles).Returns(new List<string> { "unauthorized-role" });
        _identityServiceMock
            .Setup(s => s.GetRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "unauthorized-role" });

        var handler = new ForwardNextToStepCommandHandler(_dbContext, _userMock.Object, _identityServiceMock.Object);
        var command = new ForwardNextToStepCommand { ApplicationId = app.Id };

        var ex = await Should.ThrowAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
        ex.Message.ShouldBe("User does not have permission to update the application at the current step.");
    }

    [Test]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenCurrentStepIsLastStep()
    {
        var (_, step2, stepDetail2, _, roleId) = SeedBaseData();

        var lastStep = new Step { Id = Guid.NewGuid(), Name = "Last Step", Order = 99, NextStepId = null };
        var lastStepDetail = new StepDetail { Id = Guid.NewGuid(), Name = "Last StepDetail", StepId = lastStep.Id, Order = 1, IsReturnStep = false };

        _dbContext.Steps.Add(lastStep);
        _dbContext.StepDetails.Add(lastStepDetail);
        _dbContext.RoleStepPermissions.Add(new RoleStepPermission { RoleId = roleId, StepId = lastStep.Id, Step = lastStep });

        var app = new DomainApplication
        {
            Id = Guid.NewGuid(),
            Code = "APP-001",
            Title = "Test",
            Description = "Test",
            StepDetailId = lastStepDetail.Id
        };
        _dbContext.Applications.Add(app);
        _dbContext.SaveChanges();

        SetupUserAndIdentityMocks(new List<string> { roleId }, roleId);

        var handler = new ForwardNextToStepCommandHandler(_dbContext, _userMock.Object, _identityServiceMock.Object);
        var command = new ForwardNextToStepCommand { ApplicationId = app.Id };

        var ex = await Should.ThrowAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        ex.Message.ShouldBe("Hồ sơ đã ở bước cuối cùng của quy trình.");
    }

    [Test]
    public async Task Handle_ShouldForwardToNextStep_WhenAllConditionsMet()
    {
        var (_, _, stepDetail1, stepDetail2, roleId) = SeedBaseData();

        var app = new DomainApplication
        {
            Id = Guid.NewGuid(),
            Code = "APP-001",
            Title = "Test",
            Description = "Test",
            StepDetailId = stepDetail1.Id
        };
        _dbContext.Applications.Add(app);
        _dbContext.SaveChanges();

        SetupUserAndIdentityMocks(new List<string> { roleId }, roleId);

        var handler = new ForwardNextToStepCommandHandler(_dbContext, _userMock.Object, _identityServiceMock.Object);
        var command = new ForwardNextToStepCommand { ApplicationId = app.Id };

        var result = await handler.Handle(command, CancellationToken.None);

        result.ShouldBe(app.Id);

        var updatedApp = await _dbContext.Applications.FindAsync(app.Id);
        updatedApp!.StepDetailId.ShouldBe(stepDetail2.Id);
    }
}
