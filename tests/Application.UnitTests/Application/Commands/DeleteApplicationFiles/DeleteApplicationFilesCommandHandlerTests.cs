using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RMS.Application.Application.Commands.DeleteApplicationFiles;
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

public class DeleteApplicationFilesCommandHandlerTests : IDisposable
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

    private Step CreateStep(Guid id, int order)
    {
        var step = new Step
        {
            Id = id,
            Name = $"Step-{order}",
            ShortName = $"S{order}",
            Order = order
        };
        _dbContext.Steps.Add(step);
        return step;
    }

    private StepDetail CreateStepDetail(Guid id, Guid stepId, int order, Step step)
    {
        var stepDetail = new StepDetail
        {
            Id = id,
            Name = $"StepDetail-{order}",
            Order = order,
            StepId = stepId,
            Step = step
        };
        _dbContext.StepDetails.Add(stepDetail);
        return stepDetail;
    }

    private DomainApplication CreateApplication(Guid id, ApplicationStatus status, StepDetail stepDetail)
    {
        var app = new DomainApplication
        {
            Id = id,
            Code = $"APP-{id:N}".Substring(0, 12),
            Title = "Test Application",
            Description = "Test Description",
            Status = status,
            StepDetailId = stepDetail.Id,
            StepDetail = stepDetail
        };
        _dbContext.Applications.Add(app);
        return app;
    }

    private async Task<RoleStepPermission> CreateRoleStepPermission(string roleId, Guid stepId, Step step)
    {
        var permission = new RoleStepPermission
        {
            RoleId = roleId,
            StepId = stepId,
            Step = step
        };
        _dbContext.RoleStepPermissions.Add(permission);
        await _dbContext.SaveChangesAsync();
        return permission;
    }

    private DomainFile CreateFile(Guid id, string path)
    {
        var file = new DomainFile
        {
            Id = id,
            Name = "test.pdf",
            ContentType = "application/pdf",
            Length = 1024,
            Path = path
        };
        _dbContext.Files.Add(file);
        return file;
    }

    private ApplicationFile CreateApplicationFile(
        Guid applicationId,
        Guid fileId,
        DomainApplication application,
        DomainFile file,
        Guid stepId,
        Step step)
    {
        var applicationFile = new ApplicationFile
        {
            ApplicationId = applicationId,
            FileId = fileId,
            Application = application,
            File = file,
            StepId = stepId,
            Step = step
        };
        _dbContext.ApplicationFiles.Add(applicationFile);
        _dbContext.SaveChanges();
        return applicationFile;
    }

    private void SetupUserMock(List<string>? roles)
    {
        _userMock.Setup(u => u.Roles).Returns(roles);
    }

    private void SetupIdentityServiceMock(string roleId)
    {
        _identityServiceMock
            .Setup(s => s.GetRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { roleId });
    }

    private DeleteApplicationFilesCommandHandler CreateHandler()
    {
        return new DeleteApplicationFilesCommandHandler(
            _dbContext,
            _fileServiceMock.Object,
            _userMock.Object,
            _identityServiceMock.Object);
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenApplicationFileNotFound()
    {
        var appId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var command = new DeleteApplicationFilesCommand { ApplicationId = appId, FileId = fileId };
        var handler = CreateHandler();

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ShouldThrowForbiddenAccessException_WhenTeacherDeletesNonDraft()
    {
        var step = CreateStep(Guid.NewGuid(), order: 1);
        var stepDetail = CreateStepDetail(Guid.NewGuid(), step.Id, order: 1, step);
        var app = CreateApplication(Guid.NewGuid(), ApplicationStatus.Submitted, stepDetail);
        var file = CreateFile(Guid.NewGuid(), "/files/test.pdf");
        CreateApplicationFile(app.Id, file.Id, app, file, step.Id, step);
        _dbContext.SaveChanges();

        SetupUserMock(new List<string> { Roles.Teacher });
        SetupIdentityServiceMock("teacher-role-id");
        await CreateRoleStepPermission("teacher-role-id", step.Id, step);

        var command = new DeleteApplicationFilesCommand { ApplicationId = app.Id, FileId = file.Id };
        var handler = CreateHandler();

        var ex = await Should.ThrowAsync<ForbiddenAccessException>(() => handler.Handle(command, CancellationToken.None));
        ex.Message.ShouldContain("Teacher can only delete files when application is in Draft status");
    }

    [Test]
    public async Task Handle_ShouldThrowForbiddenAccessException_WhenRoleNotPermitted()
    {
        var step = CreateStep(Guid.NewGuid(), order: 1);
        var stepDetail = CreateStepDetail(Guid.NewGuid(), step.Id, order: 1, step);
        var app = CreateApplication(Guid.NewGuid(), ApplicationStatus.Draft, stepDetail);
        var file = CreateFile(Guid.NewGuid(), "/files/test.pdf");
        CreateApplicationFile(app.Id, file.Id, app, file, step.Id, step);
        _dbContext.SaveChanges();

        SetupUserMock(new List<string> { Roles.Tttv });
        SetupIdentityServiceMock("tttv-role-id");

        var command = new DeleteApplicationFilesCommand { ApplicationId = app.Id, FileId = file.Id };
        var handler = CreateHandler();

        var ex = await Should.ThrowAsync<ForbiddenAccessException>(() => handler.Handle(command, CancellationToken.None));
        ex.Message.ShouldContain("Current role is not permitted to delete files for this step");
    }

    [Test]
    public async Task Handle_ShouldThrowForbiddenAccessException_WhenAppStepOrderGreaterThanTarget()
    {
        var fileStep = CreateStep(Guid.NewGuid(), order: 1);
        var appStep = CreateStep(Guid.NewGuid(), order: 2);
        var fileStepDetail = CreateStepDetail(Guid.NewGuid(), fileStep.Id, order: 1, fileStep);
        var appStepDetail = CreateStepDetail(Guid.NewGuid(), appStep.Id, order: 2, appStep);

        var app = CreateApplication(Guid.NewGuid(), ApplicationStatus.Submitted, appStepDetail);
        var file = CreateFile(Guid.NewGuid(), "/files/test.pdf");
        CreateApplicationFile(app.Id, file.Id, app, file, fileStep.Id, fileStep);
        _dbContext.SaveChanges();

        SetupUserMock(new List<string> { Roles.Administrator });
        SetupIdentityServiceMock("admin-role-id");
        await CreateRoleStepPermission("admin-role-id", fileStep.Id, fileStep);

        var command = new DeleteApplicationFilesCommand { ApplicationId = app.Id, FileId = file.Id };
        var handler = CreateHandler();

        var ex = await Should.ThrowAsync<ForbiddenAccessException>(() => handler.Handle(command, CancellationToken.None));
        ex.Message.ShouldContain("Cannot delete files from a previous step after the application has advanced");
    }

    [Test]
    public async Task Handle_ShouldDeleteApplicationFile_WhenValidRequest()
    {
        var step = CreateStep(Guid.NewGuid(), order: 1);
        var stepDetail = CreateStepDetail(Guid.NewGuid(), step.Id, order: 1, step);
        var app = CreateApplication(Guid.NewGuid(), ApplicationStatus.Draft, stepDetail);
        var file = CreateFile(Guid.NewGuid(), "/files/test.pdf");
        CreateApplicationFile(app.Id, file.Id, app, file, step.Id, step);
        _dbContext.SaveChanges();

        SetupUserMock(new List<string> { Roles.Teacher });
        SetupIdentityServiceMock("teacher-role-id");
        await CreateRoleStepPermission("teacher-role-id", step.Id, step);

        var command = new DeleteApplicationFilesCommand { ApplicationId = app.Id, FileId = file.Id };
        var handler = CreateHandler();

        await handler.Handle(command, CancellationToken.None);

        var deletedAppFile = await _dbContext.ApplicationFiles
            .FindAsync(app.Id, file.Id);
        deletedAppFile.ShouldBeNull();

        var deletedFile = await _dbContext.Files.FindAsync(file.Id);
        deletedFile.ShouldBeNull();

        _fileServiceMock.Verify(
            f => f.DeleteFile("/files/test.pdf", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
