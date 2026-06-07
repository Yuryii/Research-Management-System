using System.Collections.Generic;
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

namespace RMS.Application.UnitTests.Application.Commands;

public class CreateApplicationCommandHandlerTests : IDisposable
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<IApplicationFileService> _applicationFileServiceMock = null!;
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
        _applicationFileServiceMock = new Mock<IApplicationFileService>();
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

    private static Mock<IFormFileCollection> CreateMockFormFileCollection()
    {
        var mockCollection = new Mock<IFormFileCollection>();
        mockCollection.Setup(c => c.Count).Returns(0);
        return mockCollection;
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
            _applicationFileServiceMock.Object,
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

        _applicationFileServiceMock.Verify(
            f => f.AddFilesToApplicationAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<IFormFileCollection>(),
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
            _applicationFileServiceMock.Object,
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
    public async Task Handle_ShouldCallAddFilesToApplicationAsync_WhenFilesProvided()
    {
        AddStepDetail();

        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("document.pdf");
        mockFile.Setup(f => f.ContentType).Returns("application/pdf");
        mockFile.Setup(f => f.Length).Returns(2048);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        var mockCollection = new Mock<IFormFileCollection>();
        var files = new List<IFormFile> { mockFile.Object };
        mockCollection.Setup(c => c.Count).Returns(1);
        mockCollection.Setup(c => c.GetEnumerator()).Returns(files.GetEnumerator());
        mockCollection.Setup(c => c[0]).Returns(mockFile.Object);

        var command = new CreateApplicationCommand
        {
            Title = "Test Application",
            Description = "Test Description",
            Status = ApplicationStatus.Draft,
            Files = mockCollection.Object
        };

        var handler = new CreateApplicationCommandHandler(
            _dbContext,
            _applicationFileServiceMock.Object,
            _codeGeneratorServiceMock.Object,
            _stepResolverMock.Object,
            _senderMock.Object,
            _userMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        _applicationFileServiceMock.Verify(
            f => f.AddFilesToApplicationAsync(
                result,
                _stepId,
                mockCollection.Object,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_ShouldRethrow_WhenApplicationFileServiceThrows()
    {
        AddStepDetail();

        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("document.pdf");

        var mockCollection = new Mock<IFormFileCollection>();
        var files = new List<IFormFile> { mockFile.Object };
        mockCollection.Setup(c => c.Count).Returns(1);
        mockCollection.Setup(c => c.GetEnumerator()).Returns(files.GetEnumerator());

        _applicationFileServiceMock
            .Setup(f => f.AddFilesToApplicationAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<IFormFileCollection>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Simulated failure"));

        var command = new CreateApplicationCommand
        {
            Title = "Test Application",
            Description = "Test Description",
            Status = ApplicationStatus.Draft,
            Files = mockCollection.Object
        };

        var handler = new CreateApplicationCommandHandler(
            _dbContext,
            _applicationFileServiceMock.Object,
            _codeGeneratorServiceMock.Object,
            _stepResolverMock.Object,
            _senderMock.Object,
            _userMock.Object);

        await Should.ThrowAsync<DbUpdateException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
