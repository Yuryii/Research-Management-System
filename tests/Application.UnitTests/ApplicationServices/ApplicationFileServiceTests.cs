using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Entities;
using RMS.Domain.Entities.Models;
using RMS.Infrastructure.Data;
using RMS.Infrastructure.Services;
using Shouldly;
using DomainFile = RMS.Domain.Entities.Models.File;

namespace RMS.Application.UnitTests.ApplicationServices;

public class ApplicationFileServiceTests : IDisposable
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<IFileService> _fileServiceMock = null!;
    private ApplicationFileService _service = null!;

    private readonly Guid _applicationId = Guid.NewGuid();
    private readonly Guid _stepId = Guid.NewGuid();

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
        _service = new ApplicationFileService(_dbContext, _fileServiceMock.Object);
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

    [Test]
    public async Task AddFilesToApplicationAsync_ShouldSaveFilesAndCreateApplicationFileRecords()
    {
        var files = CreateFormFileCollection(
            ("document1.pdf", "application/pdf", 1024),
            ("document2.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", 2048));
        var savedPaths = new List<string> { "files/applications/doc1.pdf", "files/applications/doc2.docx" };

        _fileServiceMock
            .Setup(f => f.SaveFilesAsync(
                It.IsAny<IReadOnlyList<IFormFile>>(),
                It.IsAny<HashSet<string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedPaths);

        await _service.AddFilesToApplicationAsync(_applicationId, _stepId, files, CancellationToken.None);

        var appFiles = await _dbContext.ApplicationFiles
            .Include(af => af.File)
            .ToListAsync();

        appFiles.Count.ShouldBe(2);

        appFiles[0].ApplicationId.ShouldBe(_applicationId);
        appFiles[0].StepId.ShouldBe(_stepId);
        appFiles[0].File.Name.ShouldBe("document1.pdf");
        appFiles[0].File.ContentType.ShouldBe("application/pdf");
        appFiles[0].File.Path.ShouldBe("files/applications/doc1.pdf");

        appFiles[1].ApplicationId.ShouldBe(_applicationId);
        appFiles[1].StepId.ShouldBe(_stepId);
        appFiles[1].File.Name.ShouldBe("document2.docx");
        appFiles[1].File.ContentType.ShouldBe("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
        appFiles[1].File.Path.ShouldBe("files/applications/doc2.docx");
    }

    [Test]
    public async Task AddFilesToApplicationAsync_ShouldCallSaveFilesAsync_WithCorrectParameters()
    {
        var files = CreateFormFileCollection(("test.pdf", "application/pdf", 1024));
        var savedPaths = new List<string> { "files/applications/test.pdf" };

        _fileServiceMock
            .Setup(f => f.SaveFilesAsync(
                It.IsAny<IReadOnlyList<IFormFile>>(),
                It.IsAny<HashSet<string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedPaths);

        await _service.AddFilesToApplicationAsync(_applicationId, _stepId, files, CancellationToken.None);

        _fileServiceMock.Verify(
            f => f.SaveFilesAsync(
                It.Is<IReadOnlyList<IFormFile>>(list => list.Count == 1),
                It.IsAny<HashSet<string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task AddFilesToApplicationAsync_ShouldDeleteFiles_WhenSaveChangesThrows()
    {
        var files = CreateFormFileCollection(("test.pdf", "application/pdf", 1024));
        var savedPaths = new List<string> { "files/applications/test.pdf" };

        _fileServiceMock
            .Setup(f => f.SaveFilesAsync(
                It.IsAny<IReadOnlyList<IFormFile>>(),
                It.IsAny<HashSet<string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedPaths);

        var failingContextMock = new Mock<IApplicationDbContext>();
        failingContextMock.Setup(c => c.ApplicationFiles).Returns(_dbContext.ApplicationFiles);
        failingContextMock.Setup(c => c.Files).Returns(_dbContext.Files);
        failingContextMock
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Simulated failure"));

        var failingService = new ApplicationFileService(failingContextMock.Object, _fileServiceMock.Object);

        await Should.ThrowAsync<DbUpdateException>(() =>
            failingService.AddFilesToApplicationAsync(_applicationId, _stepId, files, CancellationToken.None));

        _fileServiceMock.Verify(
            f => f.DeleteFile("files/applications/test.pdf", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task AddFilesToApplicationAsync_ShouldDeleteAllSavedFiles_WhenSaveChangesThrowsWithMultipleFiles()
    {
        var files = CreateFormFileCollection(
            ("doc1.pdf", "application/pdf", 1024),
            ("doc2.pdf", "application/pdf", 2048));
        var savedPaths = new List<string> { "files/applications/doc1.pdf", "files/applications/doc2.pdf" };

        _fileServiceMock
            .Setup(f => f.SaveFilesAsync(
                It.IsAny<IReadOnlyList<IFormFile>>(),
                It.IsAny<HashSet<string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedPaths);

        var failingContextMock = new Mock<IApplicationDbContext>();
        failingContextMock.Setup(c => c.ApplicationFiles).Returns(_dbContext.ApplicationFiles);
        failingContextMock.Setup(c => c.Files).Returns(_dbContext.Files);
        failingContextMock
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Simulated failure"));

        var failingService = new ApplicationFileService(failingContextMock.Object, _fileServiceMock.Object);

        await Should.ThrowAsync<DbUpdateException>(() =>
            failingService.AddFilesToApplicationAsync(_applicationId, _stepId, files, CancellationToken.None));

        _fileServiceMock.Verify(
            f => f.DeleteFile(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }
}
