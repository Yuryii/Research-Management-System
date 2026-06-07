using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RMS.Application.Application.Commands.CreateApplicationFiles;
using RMS.Application.Common.Exceptions;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Constants;
using RMS.Domain.Entities;
using RMS.Domain.Entities.Models;
using RMS.Domain.Enums;
using RMS.Infrastructure.Data;
using Shouldly;
using DomainApplication = RMS.Domain.Entities.Models.Application;
using DomainFile = RMS.Domain.Entities.Models.File;
using NotFoundException = Ardalis.GuardClauses.NotFoundException;

namespace RMS.Application.UnitTests.Application.Commands;

public class CreateApplicationFilesCommandHandlerTests : IDisposable
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<IApplicationFileService> _applicationFileServiceMock = null!;
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
        _applicationFileServiceMock = new Mock<IApplicationFileService>();
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

    private (DomainApplication App, Step Step, string RoleId) SetupApplicationWithStepAndDetail(
        ApplicationStatus status,
        string roleName,
        bool addPermission)
    {
        var step = new Step
        {
            Id = Guid.NewGuid(),
            Name = "Step 1",
            ShortName = "S1",
            Order = 1
        };
        _dbContext.Steps.Add(step);

        var stepDetail = new StepDetail
        {
            Id = Guid.NewGuid(),
            Name = "StepDetail 1",
            Order = 1,
            StepId = step.Id,
            Step = step
        };
        _dbContext.StepDetails.Add(stepDetail);

        var app = new DomainApplication
        {
            Id = Guid.NewGuid(),
            Code = "APP-001",
            Title = "Test Application",
            Description = "Test Description",
            Status = status,
            StepDetailId = stepDetail.Id,
            StepDetail = stepDetail
        };
        _dbContext.Applications.Add(app);

        string roleId = "role-tttvid";
        if (addPermission)
        {
            _dbContext.RoleStepPermissions.Add(new RoleStepPermission
            {
                RoleId = roleId,
                StepId = step.Id
            });
        }

        _dbContext.SaveChanges();

        return (app, step, roleId);
    }

    private static IFormFileCollection CreateFormFileCollection(params (string name, string contentType, long length)[] files)
    {
        var collection = new FormFileCollection();
        foreach (var (name, contentType, length) in files)
        {
            var stream = new MemoryStream(new byte[length]);
            collection.Add(new FormFile(stream, 0, length, "Files", name)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            });
        }
        return collection;
    }

    private void SetupUserAndIdentity(string roleName, string roleId)
    {
        _userMock.Setup(u => u.Id).Returns("user-123");
        _userMock.Setup(u => u.Roles).Returns(new List<string> { roleName });
        _identityServiceMock
            .Setup(s => s.GetRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { roleId });
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenApplicationDoesNotExist()
    {
        var handler = new CreateApplicationFilesCommandHandler(
            _dbContext, _applicationFileServiceMock.Object, _userMock.Object, _identityServiceMock.Object);
        var command = new CreateApplicationFilesCommand
        {
            ApplicationId = Guid.NewGuid(),
            Files = CreateFormFileCollection(("test.pdf", "application/pdf", 1024))
        };

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ShouldThrowForbiddenAccessException_WhenTeacherUploadsNonDraftApplication()
    {
        var (app, step, roleId) = SetupApplicationWithStepAndDetail(
            ApplicationStatus.Submitted, Roles.Teacher, addPermission: true);
        _userMock.Setup(u => u.Id).Returns("user-123");
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Teacher });
        _identityServiceMock
            .Setup(s => s.GetRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { roleId });

        var handler = new CreateApplicationFilesCommandHandler(
            _dbContext, _applicationFileServiceMock.Object, _userMock.Object, _identityServiceMock.Object);
        var command = new CreateApplicationFilesCommand
        {
            ApplicationId = app.Id,
            Files = CreateFormFileCollection(("test.pdf", "application/pdf", 1024))
        };

        var ex = await Should.ThrowAsync<ForbiddenAccessException>(() => handler.Handle(command, CancellationToken.None));
        ex.Message.ShouldBe("Teacher can only upload files when application is in Draft status.");
    }

    [Test]
    public async Task Handle_ShouldThrowForbiddenAccessException_WhenRoleNotPermitted()
    {
        var (app, step, roleId) = SetupApplicationWithStepAndDetail(
            ApplicationStatus.Draft, Roles.Tttv, addPermission: false);
        _userMock.Setup(u => u.Id).Returns("user-123");
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Tttv });
        _identityServiceMock
            .Setup(s => s.GetRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { roleId });

        var handler = new CreateApplicationFilesCommandHandler(
            _dbContext, _applicationFileServiceMock.Object, _userMock.Object, _identityServiceMock.Object);
        var command = new CreateApplicationFilesCommand
        {
            ApplicationId = app.Id,
            Files = CreateFormFileCollection(("test.pdf", "application/pdf", 1024))
        };

        var ex = await Should.ThrowAsync<ForbiddenAccessException>(() => handler.Handle(command, CancellationToken.None));
        ex.Message.ShouldBe("Current role is not permitted to upload files for this step.");
    }

    [Test]
    public async Task Handle_ShouldThrowForbiddenAccessException_WhenUserHasNoRoles()
    {
        var (app, step, roleId) = SetupApplicationWithStepAndDetail(
            ApplicationStatus.Draft, Roles.Tttv, addPermission: false);
        _userMock.Setup(u => u.Id).Returns("user-123");
        _userMock.Setup(u => u.Roles).Returns(new List<string>());
        _identityServiceMock
            .Setup(s => s.GetRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        var handler = new CreateApplicationFilesCommandHandler(
            _dbContext, _applicationFileServiceMock.Object, _userMock.Object, _identityServiceMock.Object);
        var command = new CreateApplicationFilesCommand
        {
            ApplicationId = app.Id,
            Files = CreateFormFileCollection(("test.pdf", "application/pdf", 1024))
        };

        var ex = await Should.ThrowAsync<ForbiddenAccessException>(() => handler.Handle(command, CancellationToken.None));
        ex.Message.ShouldBe("Current role is not permitted to upload files for this step.");
    }

    [Test]
    public async Task Handle_ShouldCallAddFilesToApplicationAsync_WhenTeacherUploadsDraftApplication()
    {
        var (app, step, roleId) = SetupApplicationWithStepAndDetail(
            ApplicationStatus.Draft, Roles.Teacher, addPermission: true);
        _userMock.Setup(u => u.Id).Returns("user-123");
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Teacher });
        _identityServiceMock
            .Setup(s => s.GetRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { roleId });

        var files = CreateFormFileCollection(
            ("test1.pdf", "application/pdf", 1024),
            ("test2.pdf", "application/pdf", 2048));

        var handler = new CreateApplicationFilesCommandHandler(
            _dbContext, _applicationFileServiceMock.Object, _userMock.Object, _identityServiceMock.Object);
        var command = new CreateApplicationFilesCommand
        {
            ApplicationId = app.Id,
            Files = files
        };

        await handler.Handle(command, CancellationToken.None);

        _applicationFileServiceMock.Verify(
            f => f.AddFilesToApplicationAsync(
                app.Id,
                step.Id,
                files,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_ShouldCallAddFilesToApplicationAsync_WhenAuthorizedRoleUploads()
    {
        var (app, step, roleId) = SetupApplicationWithStepAndDetail(
            ApplicationStatus.Submitted, Roles.Tttv, addPermission: true);
        _userMock.Setup(u => u.Id).Returns("user-123");
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Tttv });
        _identityServiceMock
            .Setup(s => s.GetRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { roleId });

        var files = CreateFormFileCollection(("doc.pdf", "application/pdf", 512));

        var handler = new CreateApplicationFilesCommandHandler(
            _dbContext, _applicationFileServiceMock.Object, _userMock.Object, _identityServiceMock.Object);
        var command = new CreateApplicationFilesCommand
        {
            ApplicationId = app.Id,
            Files = files
        };

        await handler.Handle(command, CancellationToken.None);

        _applicationFileServiceMock.Verify(
            f => f.AddFilesToApplicationAsync(
                app.Id,
                step.Id,
                files,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_ShouldRethrow_WhenApplicationFileServiceThrows()
    {
        var (app, step, roleId) = SetupApplicationWithStepAndDetail(
            ApplicationStatus.Draft, Roles.Teacher, addPermission: true);
        _userMock.Setup(u => u.Id).Returns("user-123");
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Teacher });
        _identityServiceMock
            .Setup(s => s.GetRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { roleId });

        _applicationFileServiceMock
            .Setup(f => f.AddFilesToApplicationAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<IFormFileCollection>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Simulated save failure"));

        var handler = new CreateApplicationFilesCommandHandler(
            _dbContext, _applicationFileServiceMock.Object, _userMock.Object, _identityServiceMock.Object);
        var command = new CreateApplicationFilesCommand
        {
            ApplicationId = app.Id,
            Files = CreateFormFileCollection(("test.pdf", "application/pdf", 1024))
        };

        await Should.ThrowAsync<DbUpdateException>(() => handler.Handle(command, CancellationToken.None));
    }
}
