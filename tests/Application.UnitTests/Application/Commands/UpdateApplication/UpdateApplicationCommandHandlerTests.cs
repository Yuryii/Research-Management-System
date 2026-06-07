using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RMS.Application.Application.Commands.ForwardNextToStep;
using RMS.Application.Application.Commands.UpdateApplication;
using RMS.Application.Common.Exceptions;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Entities.Models;
using RMS.Domain.Enums;
using RMS.Infrastructure.Data;
using Shouldly;
using DomainApplication = RMS.Domain.Entities.Models.Application;
using NotFoundException = Ardalis.GuardClauses.NotFoundException;

namespace RMS.Application.UnitTests.Application.Commands;

public class UpdateApplicationCommandHandlerTests : IDisposable
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<ISender> _senderMock = null!;
    private string _tempDir = null!;

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
        _senderMock = new Mock<ISender>();
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
        if (_tempDir != null && Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
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
            var filePath = Path.Combine(_tempDir, $"file-{Guid.NewGuid()}.pdf");
            System.IO.File.WriteAllText(filePath, "dummy content");

            var file = new Domain.Entities.Models.File
            {
                Id = Guid.NewGuid(),
                Name = $"file-{i}.pdf",
                ContentType = "application/pdf",
                Length = 1024,
                Path = filePath
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
        var command = new UpdateApplicationCommand { Id = appId, Title = "Updated Title" };
        var handler = new UpdateApplicationCommandHandler(_dbContext, _senderMock.Object);

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ShouldThrowValidationException_WhenApplicationIsNotDraft()
    {
        var app = AddApplication(ApplicationStatus.Submitted);
        var command = new UpdateApplicationCommand { Id = app.Id, Title = "Updated Title" };
        var handler = new UpdateApplicationCommandHandler(_dbContext, _senderMock.Object);

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ShouldUpdateTitle_WhenApplicationIsDraft()
    {
        var app = AddApplication(ApplicationStatus.Draft);
        var command = new UpdateApplicationCommand { Id = app.Id, Title = "Updated Title" };
        var handler = new UpdateApplicationCommandHandler(_dbContext, _senderMock.Object);

        await handler.Handle(command, CancellationToken.None);

        var updated = await _dbContext.Applications.FindAsync(app.Id);
        updated!.Title.ShouldBe("Updated Title");
    }

    [Test]
    public async Task Handle_ShouldUpdateDescription_WhenApplicationIsDraft()
    {
        var app = AddApplication(ApplicationStatus.Draft);
        var command = new UpdateApplicationCommand { Id = app.Id, Description = "Updated Description" };
        var handler = new UpdateApplicationCommandHandler(_dbContext, _senderMock.Object);

        await handler.Handle(command, CancellationToken.None);

        var updated = await _dbContext.Applications.FindAsync(app.Id);
        updated!.Description.ShouldBe("Updated Description");
    }

    [Test]
    public async Task Handle_ShouldSendForwardNextToStepCommand_WhenStatusIsSubmitted()
    {
        var app = AddApplication(ApplicationStatus.Draft);
        var command = new UpdateApplicationCommand { Id = app.Id, Status = ApplicationStatus.Submitted };
        var handler = new UpdateApplicationCommandHandler(_dbContext, _senderMock.Object);

        await handler.Handle(command, CancellationToken.None);

        _senderMock.Verify(
            s => s.Send(It.Is<ForwardNextToStepCommand>(c => c.ApplicationId == app.Id), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_ShouldRemoveFiles_WhenFileIdsProvided()
    {
        var app = AddApplication(ApplicationStatus.Draft);
        AddFilesToApplication(app, 3);

        var remainingFileId = (await _dbContext.ApplicationFiles.FirstAsync(af => af.ApplicationId == app.Id)).FileId;
        var command = new UpdateApplicationCommand { Id = app.Id, FileIds = new List<Guid> { remainingFileId } };
        var handler = new UpdateApplicationCommandHandler(_dbContext, _senderMock.Object);

        await handler.Handle(command, CancellationToken.None);

        var remainingFiles = await _dbContext.ApplicationFiles.Where(af => af.ApplicationId == app.Id).ToListAsync();
        remainingFiles.Count.ShouldBe(1);
        remainingFiles[0].FileId.ShouldBe(remainingFileId);
    }

    [Test]
    public async Task Handle_ShouldSaveChanges_WhenUpdatingDraftApplication()
    {
        var app = AddApplication(ApplicationStatus.Draft);
        var command = new UpdateApplicationCommand { Id = app.Id, Title = "New Title", Description = "New Description" };
        var handler = new UpdateApplicationCommandHandler(_dbContext, _senderMock.Object);

        await handler.Handle(command, CancellationToken.None);

        var updated = await _dbContext.Applications.FindAsync(app.Id);
        updated!.Title.ShouldBe("New Title");
        updated.Description.ShouldBe("New Description");
    }
}
