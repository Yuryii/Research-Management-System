using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RMS.Application.Common.Interfaces;
using RMS.Application.Steps.Dtos;
using RMS.Application.Steps.Queries.GetMySteps;
using RMS.Domain.Constants;
using RMS.Domain.Entities;
using RMS.Infrastructure.Data;
using Shouldly;
using DomainStep = RMS.Domain.Entities.Models.Step;
using DomainStepDetail = RMS.Domain.Entities.Models.StepDetail;

namespace RMS.Application.UnitTests.Steps.Queries.GetMySteps;

public class GetMyStepsQueryHandlerTests : IDisposable
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<IMapper> _mapperMock = null!;
    private Mock<IUser> _userMock = null!;
    private Mock<IIdentityService> _identityServiceMock = null!;

    private readonly string _teacherRoleId = Guid.NewGuid().ToString();
    private readonly string _dvqlttRoleId = Guid.NewGuid().ToString();
    private readonly string _userId = Guid.NewGuid().ToString();

    private static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static IConfigurationProvider CreateConfigurationProvider()
    {
        var expr = new MapperConfigurationExpression();
        expr.AddMaps(typeof(StepDto).Assembly);
        var config = new MapperConfiguration(expr, new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory());
        config.AssertConfigurationIsValid();
        return config;
    }

    [SetUp]
    public void SetUp()
    {
        _dbContext = CreateInMemoryContext();
        _mapperMock = new Mock<IMapper>();
        _userMock = new Mock<IUser>();
        _identityServiceMock = new Mock<IIdentityService>();

        var configProvider = CreateConfigurationProvider();
        _mapperMock.Setup(m => m.ConfigurationProvider).Returns(configProvider);
        _mapperMock.Setup(m => m.Map<IList<StepDto>>(It.IsAny<object>()))
            .Returns((IList<DomainStep>? steps) => steps == null
                ? new List<StepDto>()
                : steps.Select(s => new StepDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    ShortName = s.ShortName,
                    Order = s.Order,
                    StepDetails = s.StepDetails.OrderBy(sd => sd.Order).Select(sd => new StepDetailDto
                    {
                        Id = sd.Id,
                        StepId = sd.StepId,
                        Name = sd.Name,
                        Order = sd.Order
                    }).ToList()
                }).ToList());
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

    private DomainStep CreateStep(Guid id, string name, string shortName, int order)
    {
        var step = new DomainStep
        {
            Id = id,
            Name = name,
            ShortName = shortName,
            Order = order
        };
        _dbContext.Steps.Add(step);
        return step;
    }

    private DomainStepDetail CreateStepDetail(Guid id, Guid stepId, string name, int order)
    {
        var stepDetail = new DomainStepDetail
        {
            Id = id,
            StepId = stepId,
            Name = name,
            Order = order
        };
        _dbContext.StepDetails.Add(stepDetail);
        return stepDetail;
    }

    private void CreateRoleStepPermission(string roleId, Guid stepId)
    {
        _dbContext.RoleStepPermissions.Add(new RoleStepPermission
        {
            RoleId = roleId,
            StepId = stepId
        });
    }

    [Test]
    public async Task Handle_ShouldReturnEmptyList_WhenUserHasNoRoles()
    {
        // Arrange
        _userMock.Setup(u => u.Roles).Returns((List<string>?)null);

        var handler = new GetMyStepsQueryHandler(
            _dbContext, _mapperMock.Object, _userMock.Object, _identityServiceMock.Object);

        var query = new GetMyStepsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task Handle_ShouldReturnEmptyList_WhenUserHasEmptyRoles()
    {
        // Arrange
        _userMock.Setup(u => u.Roles).Returns(new List<string>());

        var handler = new GetMyStepsQueryHandler(
            _dbContext, _mapperMock.Object, _userMock.Object, _identityServiceMock.Object);

        var query = new GetMyStepsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task Handle_ShouldReturnStepsForTeacher_WhenUserHasTeacherRole()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        CreateStep(stepId, "Teacher Step", "TS", 1);
        CreateStepDetail(Guid.NewGuid(), stepId, "Submit", 1);
        CreateRoleStepPermission(_teacherRoleId, stepId);

        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Teacher });
        _identityServiceMock
            .Setup(s => s.GetRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { _teacherRoleId });

        _dbContext.SaveChanges();

        var handler = new GetMyStepsQueryHandler(
            _dbContext, _mapperMock.Object, _userMock.Object, _identityServiceMock.Object);

        var query = new GetMyStepsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Teacher Step");
    }

    [Test]
    public async Task Handle_ShouldReturnStepsForMultipleRoles_WhenUserHasMultipleRoles()
    {
        // Arrange
        var step1Id = Guid.NewGuid();
        var step2Id = Guid.NewGuid();
        CreateStep(step1Id, "Teacher Step", "TS", 1);
        CreateStep(step2Id, "DVQLTT Step", "DV", 2);
        CreateRoleStepPermission(_teacherRoleId, step1Id);
        CreateRoleStepPermission(_dvqlttRoleId, step2Id);

        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Teacher, Roles.Dvqltt });
        _identityServiceMock
            .Setup(s => s.GetRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { _teacherRoleId, _dvqlttRoleId });

        _dbContext.SaveChanges();

        var handler = new GetMyStepsQueryHandler(
            _dbContext, _mapperMock.Object, _userMock.Object, _identityServiceMock.Object);

        var query = new GetMyStepsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
    }

    [Test]
    public async Task Handle_ShouldReturnDistinctSteps_WhenStepIsAssignedToMultipleRoles()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        CreateStep(stepId, "Shared Step", "SS", 1);
        CreateRoleStepPermission(_teacherRoleId, stepId);
        CreateRoleStepPermission(_dvqlttRoleId, stepId);

        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Teacher, Roles.Dvqltt });
        _identityServiceMock
            .Setup(s => s.GetRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { _teacherRoleId, _dvqlttRoleId });

        _dbContext.SaveChanges();

        var handler = new GetMyStepsQueryHandler(
            _dbContext, _mapperMock.Object, _userMock.Object, _identityServiceMock.Object);

        var query = new GetMyStepsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Shared Step");
    }

    [Test]
    public async Task Handle_ShouldReturnEmptyList_WhenNoStepPermissionsExist()
    {
        // Arrange
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Teacher });
        _identityServiceMock
            .Setup(s => s.GetRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { _teacherRoleId });

        var handler = new GetMyStepsQueryHandler(
            _dbContext, _mapperMock.Object, _userMock.Object, _identityServiceMock.Object);

        var query = new GetMyStepsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task Handle_ShouldReturnStepsOrderedByOrder()
    {
        // Arrange
        var step1Id = Guid.NewGuid();
        var step2Id = Guid.NewGuid();
        var step3Id = Guid.NewGuid();
        CreateStep(step1Id, "Third Step", "TS3", 3);
        CreateStep(step2Id, "First Step", "TS1", 1);
        CreateStep(step3Id, "Second Step", "TS2", 2);
        CreateStepDetail(Guid.NewGuid(), step1Id, "Detail 1", 1);
        CreateStepDetail(Guid.NewGuid(), step2Id, "Detail 2", 1);
        CreateStepDetail(Guid.NewGuid(), step3Id, "Detail 3", 1);
        CreateRoleStepPermission(_teacherRoleId, step1Id);
        CreateRoleStepPermission(_teacherRoleId, step2Id);
        CreateRoleStepPermission(_teacherRoleId, step3Id);

        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Teacher });
        _identityServiceMock
            .Setup(s => s.GetRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { _teacherRoleId });

        _dbContext.SaveChanges();

        var handler = new GetMyStepsQueryHandler(
            _dbContext, _mapperMock.Object, _userMock.Object, _identityServiceMock.Object);

        var query = new GetMyStepsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result[0].Order.ShouldBe(1);
        result[1].Order.ShouldBe(2);
        result[2].Order.ShouldBe(3);
    }
}
