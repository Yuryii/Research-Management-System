using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RMS.Application.Application.Queries.GetApplicationFile;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Entities.Models;
using RMS.Infrastructure.Data;
using Shouldly;
using NotFoundException = Ardalis.GuardClauses.NotFoundException;
using DomainApplication = RMS.Domain.Entities.Models.Application;
using DomainFile = RMS.Domain.Entities.Models.File;

namespace RMS.Application.UnitTests.Application.Queries.GetApplicationFile;

public class GetApplicationFileQueryHandlerTests : IDisposable
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

    private void SeedData(Guid applicationId, Guid stepDetailId, Guid stepId, Guid fileId)
    {
        var step = new Step
        {
            Id = stepId,
            Name = "Test Step",
            Order = 1
        };

        var stepDetail = new StepDetail
        {
            Id = stepDetailId,
            StepId = stepId,
            Name = "Test Step Detail",
            Order = 1
        };

        var file = new DomainFile
        {
            Id = fileId,
            Name = "document.pdf",
            Path = "/files/document.pdf",
            ContentType = "application/pdf",
            Length = 2048
        };

        var application = new DomainApplication
        {
            Id = applicationId,
            Code = "APP-001",
            Title = "Test Application",
            Description = "Test Description",
            Status = Domain.Enums.ApplicationStatus.Draft,
            StepDetailId = stepDetailId
        };

        var applicationFile = new ApplicationFile
        {
            ApplicationId = applicationId,
            FileId = fileId,
            StepId = stepId
        };

        _dbContext.Steps.Add(step);
        _dbContext.StepDetails.Add(stepDetail);
        _dbContext.Files.Add(file);
        _dbContext.Applications.Add(application);
        _dbContext.ApplicationFiles.Add(applicationFile);
        _dbContext.SaveChanges();
    }

    [Test]
    public async Task Handle_ShouldReturnFileDownloadResult_WhenApplicationFileExists()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var stepDetailId = Guid.NewGuid();
        var stepId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        SeedData(applicationId, stepDetailId, stepId, fileId);

        var stream = new MemoryStream();
        _fileServiceMock
            .Setup(f => f.GetFileAsync("/files/document.pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync(stream);

        var command = new GetApplicationFileQuery(applicationId, fileId);
        var handler = new GetApplicationFileQueryHandler(_dbContext, _fileServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result!.ContentType.ShouldBe("application/pdf");
        result.FileName.ShouldBe("document.pdf");
        result.FileLength.ShouldBe(2048);
        result.Stream.ShouldBeSameAs(stream);
    }

    [Test]
    public async Task Handle_ShouldReturnNull_WhenApplicationFileNotFound()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        var command = new GetApplicationFileQuery(applicationId, fileId);
        var handler = new GetApplicationFileQueryHandler(_dbContext, _fileServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task Handle_ShouldReturnNull_WhenApplicationIdDoesNotMatch()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var differentAppId = Guid.NewGuid();
        var stepDetailId = Guid.NewGuid();
        var stepId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        SeedData(applicationId, stepDetailId, stepId, fileId);

        var command = new GetApplicationFileQuery(differentAppId, fileId);
        var handler = new GetApplicationFileQueryHandler(_dbContext, _fileServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task Handle_ShouldReturnNull_WhenFileIdDoesNotMatch()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var stepDetailId = Guid.NewGuid();
        var stepId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var differentFileId = Guid.NewGuid();

        SeedData(applicationId, stepDetailId, stepId, fileId);

        var command = new GetApplicationFileQuery(applicationId, differentFileId);
        var handler = new GetApplicationFileQueryHandler(_dbContext, _fileServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task Handle_ShouldCallGetFileAsync_WithCorrectPath()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var stepDetailId = Guid.NewGuid();
        var stepId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var expectedPath = "/files/document.pdf";

        SeedData(applicationId, stepDetailId, stepId, fileId);

        var stream = new MemoryStream();
        _fileServiceMock
            .Setup(f => f.GetFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stream);

        var command = new GetApplicationFileQuery(applicationId, fileId);
        var handler = new GetApplicationFileQueryHandler(_dbContext, _fileServiceMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _fileServiceMock.Verify(
            f => f.GetFileAsync(expectedPath, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
