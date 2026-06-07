using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RMS.Application.Application.Commands.ReturnApplication;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Entities;
using RMS.Domain.Entities.Models;
using RMS.Domain.Enums;
using RMS.Infrastructure.Data;
using Shouldly;
using DomainApplication = RMS.Domain.Entities.Models.Application;
using NotFoundException = Ardalis.GuardClauses.NotFoundException;

namespace RMS.Application.UnitTests.Application.Commands;

public class ReturnApplicationCommandHandlerTests : IDisposable
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

    private StepDetail AddReturnStepDetail()
    {
        var stepDetail = new StepDetail
        {
            Id = Guid.NewGuid(),
            Name = "ReturnStep",
            Order = 1,
            StepId = Guid.NewGuid(),
            IsReturnStep = true
        };
        _dbContext.StepDetails.Add(stepDetail);
        _dbContext.SaveChanges();
        return stepDetail;
    }

    private DomainApplication AddApplication(string createdBy = "teacher-001", StepDetail? stepDetail = null)
    {
        var stepDetailUsed = stepDetail ?? AddReturnStepDetail();
        var app = new DomainApplication
        {
            Id = Guid.NewGuid(),
            Code = "APP-001",
            Title = "Test Application",
            Description = "Test Description",
            Status = ApplicationStatus.Submitted,
            StepDetailId = stepDetailUsed.Id,
            CreatedBy = createdBy
        };
        _dbContext.Applications.Add(app);
        _dbContext.SaveChanges();
        return app;
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenApplicationDoesNotExist()
    {
        var appId = Guid.NewGuid();
        var command = new ReturnApplicationCommand
        {
            ApplicationId = appId,
            Title = "Return Title",
            Description = "Return Description"
        };
        var handler = new ReturnApplicationCommandHandler(_dbContext, _fileServiceMock.Object);

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenReturnStepDetailNotFound()
    {
        var stepDetail = new StepDetail
        {
            Id = Guid.NewGuid(),
            Name = "NonReturnStep",
            Order = 1,
            StepId = Guid.NewGuid(),
            IsReturnStep = false
        };
        _dbContext.StepDetails.Add(stepDetail);
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

        var command = new ReturnApplicationCommand
        {
            ApplicationId = app.Id,
            Title = "Return Title",
            Description = "Return Description"
        };
        var handler = new ReturnApplicationCommandHandler(_dbContext, _fileServiceMock.Object);

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ShouldCreateApplicationReturn_AndUpdateApplicationStepDetail()
    {
        var returnStepDetail = AddReturnStepDetail();
        var app = AddApplication(stepDetail: returnStepDetail);

        var command = new ReturnApplicationCommand
        {
            ApplicationId = app.Id,
            Title = "Return Title",
            Description = "Return Description"
        };
        var handler = new ReturnApplicationCommandHandler(_dbContext, _fileServiceMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.ShouldNotBe(Guid.Empty);

        var applicationReturn = await _dbContext.ApplicationReturns.FindAsync(result);
        applicationReturn.ShouldNotBeNull();
        applicationReturn!.Title.ShouldBe("Return Title");
        applicationReturn.Description.ShouldBe("Return Description");

        var updatedApp = await _dbContext.Applications.FindAsync(app.Id);
        updatedApp!.StepDetailId.ShouldBe(returnStepDetail.Id);
    }

    [Test]
    public async Task Handle_ShouldSetRecipientIdToApplicationCreatedBy_WhenRecipientIdIsNull()
    {
        var app = AddApplication(createdBy: "teacher-001");

        var command = new ReturnApplicationCommand
        {
            ApplicationId = app.Id,
            Title = "Return Title",
            Description = "Return Description",
            RecipientId = null
        };
        var handler = new ReturnApplicationCommandHandler(_dbContext, _fileServiceMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        var applicationReturn = await _dbContext.ApplicationReturns.FindAsync(result);
        applicationReturn!.RecipientId.ShouldBe("teacher-001");
    }

    [Test]
    public async Task Handle_ShouldUseProvidedRecipientId_WhenRecipientIdIsNotNull()
    {
        var app = AddApplication(createdBy: "teacher-001");

        var command = new ReturnApplicationCommand
        {
            ApplicationId = app.Id,
            Title = "Return Title",
            Description = "Return Description",
            RecipientId = "admin-001"
        };
        var handler = new ReturnApplicationCommandHandler(_dbContext, _fileServiceMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        var applicationReturn = await _dbContext.ApplicationReturns.FindAsync(result);
        applicationReturn!.RecipientId.ShouldBe("admin-001");
    }

    [Test]
    public async Task Handle_ShouldSaveFilesAndCreateApplicationReturnFiles_WhenFilesProvided()
    {
        var app = AddApplication();
        var savedFilePaths = new List<string> { "/Upload/Application/test-file.pdf" };

        _fileServiceMock
            .Setup(f => f.SaveFilesAsync(
                It.IsAny<IReadOnlyList<IFormFile>>(),
                It.IsAny<HashSet<string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedFilePaths);

        var files = CreateFormFileCollection();
        var command = new ReturnApplicationCommand
        {
            ApplicationId = app.Id,
            Title = "Return Title",
            Description = "Return Description",
            Files = files
        };
        var handler = new ReturnApplicationCommandHandler(_dbContext, _fileServiceMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        _fileServiceMock.Verify(
            f => f.SaveFilesAsync(
                It.Is<IReadOnlyList<IFormFile>>(fl => fl.Count == 1),
                It.IsAny<HashSet<string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        var returnFile = await _dbContext.ApplicationReturnFiles
            .Include(arf => arf.File)
            .SingleOrDefaultAsync(arf => arf.ApplicationReturnId == result);
        returnFile.ShouldNotBeNull();
        returnFile!.File.Name.ShouldBe("test.txt");
    }

    [Test]
    public async Task Handle_ShouldDeleteSavedFiles_WhenSaveChangesAsyncThrows()
    {
        var savedFilePaths = new List<string> { "/path/file1.pdf", "/path/file2.pdf" };

        _fileServiceMock
            .Setup(f => f.SaveFilesAsync(
                It.IsAny<IReadOnlyList<IFormFile>>(),
                It.IsAny<HashSet<string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedFilePaths);

        var app = AddApplication();
        var command = new ReturnApplicationCommand
        {
            ApplicationId = app.Id,
            Title = "Return Title",
            Description = "Return Description",
            Files = CreateFormFileCollection()
        };

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.Applications).Returns(_dbContext.Applications);
        mockContext.Setup(c => c.StepDetails).Returns(_dbContext.StepDetails);
        mockContext.Setup(c => c.ApplicationReturns).Returns(_dbContext.ApplicationReturns);
        mockContext.Setup(c => c.ApplicationReturnFiles).Returns(_dbContext.ApplicationReturnFiles);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var handler = new ReturnApplicationCommandHandler(mockContext.Object, _fileServiceMock.Object);

        await Should.ThrowAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task CleanupFiles_ShouldDeleteAllSavedFilePaths_WhenExceptionIsThrown()
    {
        var savedFilePaths = new List<string> { "/path/file1.pdf", "/path/file2.pdf" };

        foreach (var path in savedFilePaths)
        {
            _fileServiceMock.Setup(f => f.DeleteFile(path, It.IsAny<CancellationToken>()));
        }

        foreach (var path in savedFilePaths)
        {
            _fileServiceMock.Object.DeleteFile(path, CancellationToken.None);
        }

        _fileServiceMock.Verify(
            f => f.DeleteFile(It.Is<string>(p => p == savedFilePaths[0]), It.IsAny<CancellationToken>()),
            Times.Once);
        _fileServiceMock.Verify(
            f => f.DeleteFile(It.Is<string>(p => p == savedFilePaths[1]), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static FormFileCollection CreateFormFileCollection()
    {
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Test content"));
        var files = new FormFileCollection();
        files.Add(new FormFile(stream, 0, stream.Length, "Files", "test.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        });
        return files;
    }
}
