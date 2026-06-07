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
    private Mock<IFileService> _fileServiceMock = null!;
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
        _fileServiceMock = new Mock<IFileService>();
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
            _dbContext, _fileServiceMock.Object, _userMock.Object, _identityServiceMock.Object);
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
            _dbContext, _fileServiceMock.Object, _userMock.Object, _identityServiceMock.Object);
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
            _dbContext, _fileServiceMock.Object, _userMock.Object, _identityServiceMock.Object);
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
            _dbContext, _fileServiceMock.Object, _userMock.Object, _identityServiceMock.Object);
        var command = new CreateApplicationFilesCommand
        {
            ApplicationId = app.Id,
            Files = CreateFormFileCollection(("test.pdf", "application/pdf", 1024))
        };

        var ex = await Should.ThrowAsync<ForbiddenAccessException>(() => handler.Handle(command, CancellationToken.None));
        ex.Message.ShouldBe("Current role is not permitted to upload files for this step.");
    }

    [Test]
    public async Task Handle_ShouldSaveFiles_WhenTeacherUploadsDraftApplication()
    {
        var (app, step, roleId) = SetupApplicationWithStepAndDetail(
            ApplicationStatus.Draft, Roles.Teacher, addPermission: true);
        _userMock.Setup(u => u.Id).Returns("user-123");
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Teacher });
        _identityServiceMock
            .Setup(s => s.GetRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { roleId });

        var savedPaths = new List<string> { "path/file1.pdf", "path/file2.pdf" };
        _fileServiceMock
            .Setup(f => f.SaveFilesAsync(
                It.IsAny<IReadOnlyList<IFormFile>>(),
                It.IsAny<HashSet<string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedPaths);

        var files = CreateFormFileCollection(
            ("test1.pdf", "application/pdf", 1024),
            ("test2.pdf", "application/pdf", 2048));

        var handler = new CreateApplicationFilesCommandHandler(
            _dbContext, _fileServiceMock.Object, _userMock.Object, _identityServiceMock.Object);
        var command = new CreateApplicationFilesCommand
        {
            ApplicationId = app.Id,
            Files = files
        };

        await handler.Handle(command, CancellationToken.None);

        var appFiles = await _dbContext.ApplicationFiles.ToListAsync();
        var dbFiles = await _dbContext.Files.ToListAsync();

        appFiles.Count.ShouldBe(2);
        dbFiles.Count.ShouldBe(2);

        foreach (var appFile in appFiles)
        {
            appFile.ApplicationId.ShouldBe(app.Id);
            appFile.StepId.ShouldBe(step.Id);
        }

        foreach (var dbFile in dbFiles)
        {
            dbFile.Path.ShouldBeOneOf(savedPaths.ToArray());
        }
    }

    [Test]
    public async Task Handle_ShouldSaveFiles_WhenAuthorizedRoleUploads()
    {
        var (app, step, roleId) = SetupApplicationWithStepAndDetail(
            ApplicationStatus.Submitted, Roles.Tttv, addPermission: true);
        _userMock.Setup(u => u.Id).Returns("user-123");
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Tttv });
        _identityServiceMock
            .Setup(s => s.GetRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { roleId });

        var savedPaths = new List<string> { "path/file.pdf" };
        _fileServiceMock
            .Setup(f => f.SaveFilesAsync(
                It.IsAny<IReadOnlyList<IFormFile>>(),
                It.IsAny<HashSet<string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedPaths);

        var handler = new CreateApplicationFilesCommandHandler(
            _dbContext, _fileServiceMock.Object, _userMock.Object, _identityServiceMock.Object);
        var command = new CreateApplicationFilesCommand
        {
            ApplicationId = app.Id,
            Files = CreateFormFileCollection(("doc.pdf", "application/pdf", 512))
        };

        await handler.Handle(command, CancellationToken.None);

        var appFiles = await _dbContext.ApplicationFiles.ToListAsync();
        appFiles.Count.ShouldBe(1);
        appFiles[0].ApplicationId.ShouldBe(app.Id);
        appFiles[0].StepId.ShouldBe(step.Id);
    }

    [Test]
    public async Task Handle_ShouldDeleteSavedFiles_WhenSaveChangesThrows()
    {
        var (app, step, roleId) = SetupApplicationWithStepAndDetail(
            ApplicationStatus.Draft, Roles.Teacher, addPermission: true);
        _userMock.Setup(u => u.Id).Returns("user-123");
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Teacher });
        _identityServiceMock
            .Setup(s => s.GetRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { roleId });

        var savedPaths = new List<string> { "path/file.pdf" };
        _fileServiceMock
            .Setup(f => f.SaveFilesAsync(
                It.IsAny<IReadOnlyList<IFormFile>>(),
                It.IsAny<HashSet<string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedPaths);

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.Applications).Returns(_dbContext.Applications);
        mockContext.Setup(c => c.StepDetails).Returns(_dbContext.StepDetails);
        mockContext.Setup(c => c.Files).Returns(_dbContext.Files);
        mockContext.Setup(c => c.ApplicationFiles).Returns(_dbContext.ApplicationFiles);
        mockContext.Setup(c => c.RoleStepPermissions).Returns(_dbContext.RoleStepPermissions);
        mockContext
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Simulated save failure"));

        var handler = new CreateApplicationFilesCommandHandler(
            mockContext.Object, _fileServiceMock.Object, _userMock.Object, _identityServiceMock.Object);
        var command = new CreateApplicationFilesCommand
        {
            ApplicationId = app.Id,
            Files = CreateFormFileCollection(("test.pdf", "application/pdf", 1024))
        };

        await Should.ThrowAsync<DbUpdateException>(() => handler.Handle(command, CancellationToken.None));

        _fileServiceMock.Verify(
            f => f.DeleteFile(It.Is<string>(p => savedPaths.Contains(p)), It.IsAny<CancellationToken>()),
            Times.Exactly(savedPaths.Count));
    }
}
