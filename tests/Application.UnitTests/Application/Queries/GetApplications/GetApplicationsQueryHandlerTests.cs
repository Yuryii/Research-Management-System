using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using RMS.Application.Application.Dtos;
using RMS.Application.Application.Queries.GetApplications;
using RMS.Application.Common.Interfaces;
using RMS.Application.Common.Models;
using RMS.Domain.Constants;
using RMS.Domain.Entities.Models;
using RMS.Domain.Enums;
using RMS.Infrastructure.Data;
using Shouldly;
using DomainApplication = RMS.Domain.Entities.Models.Application;

namespace RMS.Application.UnitTests.Application.Queries.GetApplications;

public class GetApplicationsQueryHandlerTests : IDisposable
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<IMapper> _mapperMock = null!;
    private IConfigurationProvider _configProvider = null!;
    private Mock<IUser> _userMock = null!;
    private Mock<IIdentityService> _identityServiceMock = null!;
    private Mock<IApplicationQueryService> _queryServiceMock = null!;

    private readonly Guid _stepId = Guid.NewGuid();
    private readonly Guid _stepDetailId = Guid.NewGuid();
    private readonly Guid _otherStepDetailId = Guid.NewGuid();
    private readonly string _userId = Guid.NewGuid().ToString();

    private static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static readonly IConfigurationProvider _sharedConfigProvider;

    static GetApplicationsQueryHandlerTests()
    {
        var expr = new MapperConfigurationExpression();
        expr.AddMaps(typeof(ApplicationDto).Assembly);
        var config = new MapperConfiguration(expr, new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory());
        config.AssertConfigurationIsValid();
        _sharedConfigProvider = config;
    }

    private static IConfigurationProvider CreateConfigurationProvider() => _sharedConfigProvider;

    [SetUp]
    public void SetUp()
    {
        _dbContext = CreateInMemoryContext();
        _configProvider = CreateConfigurationProvider();
        _userMock = new Mock<IUser>();
        _identityServiceMock = new Mock<IIdentityService>();
        _queryServiceMock = new Mock<IApplicationQueryService>();

        var mapperMock = new Mock<IMapper>();
        mapperMock.Setup(m => m.ConfigurationProvider).Returns(_configProvider);

        _queryServiceMock
            .Setup(s => s.ResolveStepContextAsync(
                It.IsAny<IReadOnlyList<string>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationQueryContext(
                _stepDetailId,
                _stepId,
                null,
                new Dictionary<Guid, List<FileDto>>(),
                new Dictionary<Guid, List<FileDto>>()));

        _identityServiceMock
            .Setup(s => s.GetFullNameAsync(It.IsAny<string>()))
            .ReturnsAsync("Teacher Name");

        _mapperMock = new Mock<IMapper>();
        _mapperMock.Setup(m => m.ConfigurationProvider).Returns(_configProvider);
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

    private void SeedSteps()
    {
        var step = new Step
        {
            Id = _stepId,
            Name = "Test Step",
            ShortName = "TS",
            Order = 1
        };

        var stepDetail = new StepDetail
        {
            Id = _stepDetailId,
            StepId = _stepId,
            Name = "Test Step Detail",
            Order = 1
        };

        var otherStepDetail = new StepDetail
        {
            Id = _otherStepDetailId,
            StepId = _stepId,
            Name = "Other Step Detail",
            Order = 2
        };

        _dbContext.Steps.Add(step);
        _dbContext.StepDetails.Add(stepDetail);
        _dbContext.StepDetails.Add(otherStepDetail);
        _dbContext.SaveChanges();
    }

    private DomainApplication CreateApplication(
        string code,
        string title,
        string description,
        ApplicationStatus status,
        Guid stepDetailId,
        string? createdBy = null)
    {
        var app = new DomainApplication
        {
            Id = Guid.NewGuid(),
            Code = code,
            Title = title,
            Description = description,
            Status = status,
            StepDetailId = stepDetailId,
            CreatedBy = createdBy
        };

        _dbContext.Applications.Add(app);
        _dbContext.SaveChanges();

        return app;
    }

    [Test]
    public async Task Handle_ShouldReturnPaginatedResults_WhenApplicationsExist()
    {
        // Arrange
        SeedSteps();
        CreateApplication("APP-001", "App One", "Desc One", ApplicationStatus.Draft, _stepDetailId);
        CreateApplication("APP-002", "App Two", "Desc Two", ApplicationStatus.Submitted, _stepDetailId);

        var query = new GetApplicationsQuery { PageNumber = 1, PageSize = 10 };
        var handler = new GetApplicationsQueryHandler(
            _dbContext, _mapperMock.Object, _userMock.Object,
            _identityServiceMock.Object, _queryServiceMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.TotalCount.ShouldBe(2);
        result.Items.Count.ShouldBe(2);
    }

    [Test]
    public async Task Handle_ShouldReturnEmpty_WhenNoApplications()
    {
        // Arrange
        SeedSteps();

        var query = new GetApplicationsQuery { PageNumber = 1, PageSize = 10 };
        var handler = new GetApplicationsQueryHandler(
            _dbContext, _mapperMock.Object, _userMock.Object,
            _identityServiceMock.Object, _queryServiceMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.TotalCount.ShouldBe(0);
        result.Items.ShouldBeEmpty();
    }

    [Test]
    public async Task Handle_ShouldFilterByStatus_WhenStatusProvided()
    {
        // Arrange
        SeedSteps();
        CreateApplication("APP-001", "App One", "Desc One", ApplicationStatus.Draft, _stepDetailId);
        CreateApplication("APP-002", "App Two", "Desc Two", ApplicationStatus.Submitted, _stepDetailId);

        var query = new GetApplicationsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            Status = ApplicationStatus.Draft
        };

        var handler = new GetApplicationsQueryHandler(
            _dbContext, _mapperMock.Object, _userMock.Object,
            _identityServiceMock.Object, _queryServiceMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.ShouldBe(1);
        result.Items[0].Status.ShouldBe(ApplicationStatus.Draft);
    }

    [Test]
    public async Task Handle_ShouldFilterBySearch_WhenSearchProvided()
    {
        // Arrange
        SeedSteps();
        CreateApplication("APP-001", "Research Proposal Alpha", "Research about AI", ApplicationStatus.Draft, _stepDetailId);
        CreateApplication("APP-002", "Budget Report Beta", "Monthly budget summary", ApplicationStatus.Draft, _stepDetailId);

        var query = new GetApplicationsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            Search = "Research"
        };

        var handler = new GetApplicationsQueryHandler(
            _dbContext, _mapperMock.Object, _userMock.Object,
            _identityServiceMock.Object, _queryServiceMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.ShouldBe(1);
        result.Items[0].Title.ShouldContain("Research");
    }

    [Test]
    public async Task Handle_ShouldFilterByStepDetailId_WhenStepDetailIdProvided()
    {
        // Arrange
        SeedSteps();
        CreateApplication("APP-001", "App One", "Desc", ApplicationStatus.Draft, _stepDetailId);
        CreateApplication("APP-002", "App Two", "Desc", ApplicationStatus.Draft, _otherStepDetailId);

        var query = new GetApplicationsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            StepDetailId = _stepDetailId
        };

        var handler = new GetApplicationsQueryHandler(
            _dbContext, _mapperMock.Object, _userMock.Object,
            _identityServiceMock.Object, _queryServiceMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.ShouldBe(1);
        result.Items[0].StepDetailId.ShouldBe(_stepDetailId);
    }

    [Test]
    public async Task Handle_ShouldReturnOnlyTeacherApplications_WhenUserIsTeacher()
    {
        // Arrange
        SeedSteps();
        var teacherId = _userId;

        _userMock.Setup(u => u.Id).Returns(teacherId);
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Teacher });

        CreateApplication("APP-001", "My App", "Desc", ApplicationStatus.Draft, _stepDetailId, teacherId);
        CreateApplication("APP-002", "Other App", "Desc", ApplicationStatus.Draft, _stepDetailId, "other-user-id");

        var query = new GetApplicationsQuery { PageNumber = 1, PageSize = 10 };
        var handler = new GetApplicationsQueryHandler(
            _dbContext, _mapperMock.Object, _userMock.Object,
            _identityServiceMock.Object, _queryServiceMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.ShouldBe(1);
        result.Items[0].CreatedBy.ShouldBe(teacherId);
    }

    [Test]
    public async Task Handle_ShouldMapToApplicationDto_WithCorrectProperties()
    {
        // Arrange
        SeedSteps();
        var createdBy = _userId;
        CreateApplication("APP-001", "My Application", "My Description", ApplicationStatus.Submitted, _stepDetailId, createdBy);

        var query = new GetApplicationsQuery { PageNumber = 1, PageSize = 10 };
        var handler = new GetApplicationsQueryHandler(
            _dbContext, _mapperMock.Object, _userMock.Object,
            _identityServiceMock.Object, _queryServiceMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items[0].Code.ShouldBe("APP-001");
        result.Items[0].Title.ShouldBe("My Application");
        result.Items[0].Description.ShouldBe("My Description");
        result.Items[0].Status.ShouldBe(ApplicationStatus.Submitted);
        result.Items[0].StepDetailId.ShouldBe(_stepDetailId);
    }

    [Test]
    public async Task Handle_ShouldRespectPageSizeAndPageNumber()
    {
        // Arrange
        SeedSteps();
        for (int i = 0; i < 15; i++)
        {
            CreateApplication($"APP-{i:D3}", $"App {i}", $"Desc {i}", ApplicationStatus.Draft, _stepDetailId);
        }

        var query = new GetApplicationsQuery { PageNumber = 2, PageSize = 5 };

        var handler = new GetApplicationsQueryHandler(
            _dbContext, _mapperMock.Object, _userMock.Object,
            _identityServiceMock.Object, _queryServiceMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.ShouldBe(15);
        result.PageNumber.ShouldBe(2);
        result.PageSize.ShouldBe(5);
        result.Items.Count.ShouldBe(5);
        result.TotalPages.ShouldBe(3);
    }
}
