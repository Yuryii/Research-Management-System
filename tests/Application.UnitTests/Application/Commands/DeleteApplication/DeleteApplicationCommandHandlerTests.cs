using System.Collections.Generic;
using System.Threading;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RMS.Application.Application.Commands.DeleteApplication;
using RMS.Application.Common.Exceptions;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Entities.Models;
using RMS.Domain.Enums;
using RMS.Infrastructure.Data;
using Shouldly;
using DomainApplication = RMS.Domain.Entities.Models.Application;
using NotFoundException = Ardalis.GuardClauses.NotFoundException;

namespace RMS.Application.UnitTests.Application.Commands;

public class DeleteApplicationCommandHandlerTests : IDisposable
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<IFileService> _fileServiceMock = null!;

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

    private DomainApplication AddApplication(ApplicationStatus status)
    {
        var app = new DomainApplication
        {
            Id = Guid.NewGuid(),
            Code = "APP-001",
            Title = "Test Application",
            Description = "Test Description",
            Status = status
        };
        _dbContext.Applications.Add(app);
        _dbContext.SaveChanges();
        return app;
    }

    private void AddFilesToApplication(DomainApplication app, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var file = new Domain.Entities.Models.File
            {
                Id = Guid.NewGuid(),
                Name = $"file-{i}.pdf",
                ContentType = "application/pdf",
                Length = 1024,
                Path = $"/files/file-{i}.pdf"
            };

            _dbContext.Files.Add(file);

            var applicationFile = new ApplicationFile
            {
                ApplicationId = app.Id,
                FileId = file.Id,
                File = file,
                Application = app,
                StepId = Guid.NewGuid()
            };

            _dbContext.ApplicationFiles.Add(applicationFile);
        }
        _dbContext.SaveChanges();
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenApplicationDoesNotExist()
    {
        var appId = Guid.NewGuid();
        var command = new DeleteApplicationCommand(appId);
        var handler = new DeleteApplicationCommandHandler(_dbContext, _fileServiceMock.Object);

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ShouldThrowForbiddenAccessException_WhenApplicationIsNotDraft()
    {
        var app = AddApplication(ApplicationStatus.Submitted);
        var command = new DeleteApplicationCommand(app.Id);
        var handler = new DeleteApplicationCommandHandler(_dbContext, _fileServiceMock.Object);

        await Should.ThrowAsync<ForbiddenAccessException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ShouldDeleteApplication_WhenApplicationIsDraft()
    {
        var app = AddApplication(ApplicationStatus.Draft);
        AddFilesToApplication(app, 2);

        var command = new DeleteApplicationCommand(app.Id);
        var handler = new DeleteApplicationCommandHandler(_dbContext, _fileServiceMock.Object);

        await handler.Handle(command, CancellationToken.None);

        var deleted = await _dbContext.Applications.FindAsync(app.Id);
        deleted.ShouldBeNull();
        _fileServiceMock.Verify(
            f => f.DeleteFile(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Test]
    public async Task Handle_ShouldDeleteApplication_WhenApplicationIsDraftWithNoFiles()
    {
        var app = AddApplication(ApplicationStatus.Draft);

        var command = new DeleteApplicationCommand(app.Id);
        var handler = new DeleteApplicationCommandHandler(_dbContext, _fileServiceMock.Object);

        await handler.Handle(command, CancellationToken.None);

        var deleted = await _dbContext.Applications.FindAsync(app.Id);
        deleted.ShouldBeNull();
        _fileServiceMock.Verify(
            f => f.DeleteFile(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_ShouldCallSaveChangesAsync_WhenApplicationIsDraft()
    {
        var app = AddApplication(ApplicationStatus.Draft);

        var command = new DeleteApplicationCommand(app.Id);
        var handler = new DeleteApplicationCommandHandler(_dbContext, _fileServiceMock.Object);

        await handler.Handle(command, CancellationToken.None);

        var deleted = await _dbContext.Applications.FindAsync(app.Id);
        deleted.ShouldBeNull();
    }
}
