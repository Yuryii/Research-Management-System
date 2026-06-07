using System.Collections.Generic;
using System.IO;
using System.Threading;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RMS.Application.Application.Commands.CreateApplication;
using RMS.Application.Application.Commands.ForwardNextToStep;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Entities;
using RMS.Domain.Entities.Models;
using RMS.Domain.Enums;
using RMS.Domain.Interfaces;
using RMS.Infrastructure.Data;
using Shouldly;
using DomainApplication = RMS.Domain.Entities.Models.Application;
using DomainFile = RMS.Domain.Entities.Models.File;

namespace RMS.Application.UnitTests.Application.Commands;

public class CreateApplicationCommandHandlerTests : IDisposable
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<IFileService> _fileServiceMock = null!;
    private Mock<ICodeGeneratorService> _codeGeneratorServiceMock = null!;
    private Mock<IStepResolver> _stepResolverMock = null!;
    private Mock<ISender> _senderMock = null!;
    private Mock<IUser> _userMock = null!;

    private readonly Guid _stepId = Guid.NewGuid();
    private readonly Guid _stepDetailId = Guid.NewGuid();
    private readonly string _userId = Guid.NewGuid().ToString();
    private const string GeneratedCode = "APP-001";

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
        _codeGeneratorServiceMock = new Mock<ICodeGeneratorService>();
        _stepResolverMock = new Mock<IStepResolver>();
        _senderMock = new Mock<ISender>();
        _userMock = new Mock<IUser>();

        _stepResolverMock
            .Setup(r => r.ResolveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_stepDetailId);

        _codeGeneratorServiceMock
            .Setup(s => s.GenerateApplicationCode(It.IsAny<string>()))
            .Returns(GeneratedCode);

        _userMock
            .Setup(u => u.Id)
            .Returns(_userId);

        _senderMock
            .Setup(s => s.Send(It.IsAny<IRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(MediatR.Unit.Value));
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

    private void AddStepDetail()
    {
        var step = new Step
        {
            Id = _stepId,
            Name = "Test Step",
            Order = 1
        };

        var stepDetail = new StepDetail
        {
            Id = _stepDetailId,
            StepId = _stepId,
            Name = "Test Step Detail",
            Order = 1,
            IsReturnStep = false
        };

        _dbContext.Steps.Add(step);
        _dbContext.StepDetails.Add(stepDetail);
        _dbContext.SaveChanges();
    }

    private static Mock<IFormFile> CreateMockFormFile(string fileName = "test.pdf", string contentType = "application/pdf", long length = 1024)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
        return mockFile;
    }

    private static Mock<IFormFileCollection> CreateMockFormFileCollection(params IFormFile[] files)
    {
        var mockCollection = new Mock<IFormFileCollection>();
        var list = new List<IFormFile>(files);
        mockCollection.Setup(c => c.Count).Returns(list.Count);
        mockCollection.Setup(c => c.GetEnumerator()).Returns(list.GetEnumerator());
        mockCollection.Setup(c => c[It.IsAny<int>()]).Returns((int i) => list[i]);
        return mockCollection;
    }

    private class FailingDbContextWrapper : IApplicationDbContext
    {
        private readonly ApplicationDbContext _inner;

        public FailingDbContextWrapper(ApplicationDbContext inner) => _inner = inner;

        public DbSet<DomainApplication> Applications => _inner.Applications;
        public DbSet<ApplicationFile> ApplicationFiles => _inner.ApplicationFiles;
        public DbSet<DomainFile> Files => _inner.Files;
        public DbSet<StepDetail> StepDetails => _inner.StepDetails;
        public DbSet<Step> Steps => _inner.Steps;
        public DbSet<TodoList> TodoLists => _inner.TodoLists;
        public DbSet<TodoItem> TodoItems => _inner.TodoItems;
        public DbSet<ApplicationReturn> ApplicationReturns => _inner.ApplicationReturns;
        public DbSet<ApplicationReturnFile> ApplicationReturnFiles => _inner.ApplicationReturnFiles;
        public DbSet<RoleStepPermission> RoleStepPermissions => _inner.RoleStepPermissions;
        public DbSet<AcademicDegree> AcademicDegrees => _inner.AcademicDegrees;
        public DbSet<ResearchHour> ResearchHours => _inner.ResearchHours;

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new DbUpdateException("Simulated save failure");
        }
    }

    [Test]
    public async Task Handle_ShouldCreateApplication_WhenStatusIsDraftWithoutFiles()
    {
        AddStepDetail();

        var command = new CreateApplicationCommand
        {
            Title = "Test Application",
            Description = "Test Description",
            Status = ApplicationStatus.Draft,
            Files = CreateMockFormFileCollection().Object
        };

        var handler = new CreateApplicationCommandHandler(
            _dbContext,
            _fileServiceMock.Object,
            _codeGeneratorServiceMock.Object,
            _stepResolverMock.Object,
            _senderMock.Object,
            _userMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.ShouldNotBe(Guid.Empty);

        var application = await _dbContext.Applications.FindAsync(result);
        application.ShouldNotBeNull();
        application.Code.ShouldBe(GeneratedCode);
        application.Title.ShouldBe("Test Application");
        application.Description.ShouldBe("Test Description");
        application.Status.ShouldBe(ApplicationStatus.Draft);
        application.StepDetailId.ShouldBe(_stepDetailId);
        application.CreatedBy.ShouldBe(_userId);

        _fileServiceMock.Verify(
            f => f.SaveFilesAsync(
                It.IsAny<IReadOnlyList<IFormFile>>(),
                It.IsAny<HashSet<string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _senderMock.Verify(
            s => s.Send(It.IsAny<IRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Handle_ShouldCallSend_WhenStatusIsSubmitted()
    {
        AddStepDetail();

        var command = new CreateApplicationCommand
        {
            Title = "Test Application",
            Description = "Test Description",
            Status = ApplicationStatus.Submitted,
            Files = CreateMockFormFileCollection().Object
        };

        var handler = new CreateApplicationCommandHandler(
            _dbContext,
            _fileServiceMock.Object,
            _codeGeneratorServiceMock.Object,
            _stepResolverMock.Object,
            _senderMock.Object,
            _userMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.ShouldNotBe(Guid.Empty);

        _senderMock.Verify(
            s => s.Send(
                It.Is<ForwardNextToStepCommand>(c => c.ApplicationId == result),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_ShouldSaveFilesAndCreateApplicationFiles_WhenFilesProvided()
    {
        AddStepDetail();

        var mockFile = CreateMockFormFile("document.pdf", "application/pdf", 2048);
        var fileCollection = CreateMockFormFileCollection(mockFile.Object);

        _fileServiceMock
            .Setup(f => f.SaveFilesAsync(
                It.IsAny<IReadOnlyList<IFormFile>>(),
                It.IsAny<HashSet<string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { "/files/applications/document.pdf" });

        var command = new CreateApplicationCommand
        {
            Title = "Test Application",
            Description = "Test Description",
            Status = ApplicationStatus.Draft,
            Files = fileCollection.Object
        };

        var handler = new CreateApplicationCommandHandler(
            _dbContext,
            _fileServiceMock.Object,
            _codeGeneratorServiceMock.Object,
            _stepResolverMock.Object,
            _senderMock.Object,
            _userMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        var applicationFiles = await _dbContext.ApplicationFiles
            .Include(af => af.File)
            .Where(af => af.ApplicationId == result)
            .ToListAsync();

        applicationFiles.ShouldHaveSingleItem();
        applicationFiles[0].StepId.ShouldBe(_stepId);
        applicationFiles[0].File.Name.ShouldBe("document.pdf");
        applicationFiles[0].File.ContentType.ShouldBe("application/pdf");
        applicationFiles[0].File.Path.ShouldBe("/files/applications/document.pdf");

        _fileServiceMock.Verify(
            f => f.SaveFilesAsync(
                It.IsAny<IReadOnlyList<IFormFile>>(),
                It.IsAny<HashSet<string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_ShouldDeleteSavedFilesAndRethrow_WhenSaveChangesFails()
    {
        AddStepDetail();

        var mockFile = CreateMockFormFile("document.pdf", "application/pdf", 2048);
        var fileCollection = CreateMockFormFileCollection(mockFile.Object);

        var savedPaths = new[] { "/files/applications/document.pdf" };

        _fileServiceMock
            .Setup(f => f.SaveFilesAsync(
                It.IsAny<IReadOnlyList<IFormFile>>(),
                It.IsAny<HashSet<string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedPaths);

        var failingContext = new FailingDbContextWrapper(_dbContext);

        var command = new CreateApplicationCommand
        {
            Title = "Test Application",
            Description = "Test Description",
            Status = ApplicationStatus.Draft,
            Files = fileCollection.Object
        };

        var handler = new CreateApplicationCommandHandler(
            failingContext,
            _fileServiceMock.Object,
            _codeGeneratorServiceMock.Object,
            _stepResolverMock.Object,
            _senderMock.Object,
            _userMock.Object);

        await Should.ThrowAsync<DbUpdateException>(() =>
            handler.Handle(command, CancellationToken.None));

        _fileServiceMock.Verify(
            f => f.DeleteFile(It.Is<string>(p => p == savedPaths[0]), It.IsAny<CancellationToken>()),
            Times.Once);

        var applicationCount = await _dbContext.Applications.CountAsync();
        applicationCount.ShouldBe(0);
    }
}
