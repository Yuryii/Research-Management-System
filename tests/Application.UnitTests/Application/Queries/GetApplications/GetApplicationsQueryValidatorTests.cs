using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using RMS.Application.Application.Queries.GetApplications;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Constants;
using RMS.Infrastructure.Data;
using Shouldly;

namespace RMS.Application.UnitTests.Application.Queries.GetApplications;

public class GetApplicationsQueryValidatorTests : IDisposable
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<IUser> _userMock = null!;
    private Mock<IIdentityService> _identityServiceMock = null!;

    private readonly string _teacherRoleId = Guid.NewGuid().ToString();

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

    [Test]
    public async Task Validate_ShouldPass_WhenPageNumberIsGreaterThanZero()
    {
        // Arrange
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Teacher });

        var validator = new GetApplicationsQueryValidator(
            _userMock.Object, _dbContext, _identityServiceMock.Object);

        var query = new GetApplicationsQuery { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task Validate_ShouldFail_WhenPageNumberIsZero()
    {
        // Arrange
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Teacher });

        var validator = new GetApplicationsQueryValidator(
            _userMock.Object, _dbContext, _identityServiceMock.Object);

        var query = new GetApplicationsQuery { PageNumber = 0, PageSize = 10 };

        // Act
        var result = await validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PageNumber");
    }

    [Test]
    public async Task Validate_ShouldFail_WhenPageNumberIsNegative()
    {
        // Arrange
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Teacher });

        var validator = new GetApplicationsQueryValidator(
            _userMock.Object, _dbContext, _identityServiceMock.Object);

        var query = new GetApplicationsQuery { PageNumber = -1, PageSize = 10 };

        // Act
        var result = await validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PageNumber");
    }

    [Test]
    public async Task Validate_ShouldFail_WhenPageSizeIsZero()
    {
        // Arrange
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Teacher });

        var validator = new GetApplicationsQueryValidator(
            _userMock.Object, _dbContext, _identityServiceMock.Object);

        var query = new GetApplicationsQuery { PageNumber = 1, PageSize = 0 };

        // Act
        var result = await validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PageSize");
    }

    [Test]
    public async Task Validate_ShouldFail_WhenPageSizeExceeds100()
    {
        // Arrange
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Teacher });

        var validator = new GetApplicationsQueryValidator(
            _userMock.Object, _dbContext, _identityServiceMock.Object);

        var query = new GetApplicationsQuery { PageNumber = 1, PageSize = 101 };

        // Act
        var result = await validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PageSize");
    }

    [Test]
    public async Task Validate_ShouldPass_WhenPageSizeIs100()
    {
        // Arrange
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Teacher });

        var validator = new GetApplicationsQueryValidator(
            _userMock.Object, _dbContext, _identityServiceMock.Object);

        var query = new GetApplicationsQuery { PageNumber = 1, PageSize = 100 };

        // Act
        var result = await validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task Validate_ShouldPass_WhenStepDetailIdIsNull()
    {
        // Arrange
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Teacher });

        var validator = new GetApplicationsQueryValidator(
            _userMock.Object, _dbContext, _identityServiceMock.Object);

        var query = new GetApplicationsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            StepDetailId = null
        };

        // Act
        var result = await validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task Validate_ShouldPass_WhenStepDetailIdIsEmptyGuid()
    {
        // Arrange
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Teacher });

        var validator = new GetApplicationsQueryValidator(
            _userMock.Object, _dbContext, _identityServiceMock.Object);

        var query = new GetApplicationsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            StepDetailId = Guid.Empty
        };

        // Act
        var result = await validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task Validate_ShouldSkipStepDetailIdValidation_WhenUserIsTeacher()
    {
        // Arrange
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Teacher });

        var validator = new GetApplicationsQueryValidator(
            _userMock.Object, _dbContext, _identityServiceMock.Object);

        var query = new GetApplicationsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            StepDetailId = Guid.NewGuid()
        };

        // Act
        var result = await validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task Validate_ShouldFail_WhenStepDetailIdIsNotEmptyGuid_AndUserIsNotTeacher_AndStepDetailNotFound()
    {
        // Arrange
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Administrator });

        var validator = new GetApplicationsQueryValidator(
            _userMock.Object, _dbContext, _identityServiceMock.Object);

        var query = new GetApplicationsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            StepDetailId = Guid.NewGuid()
        };

        // Act
        var result = await validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "StepDetailId");
    }

    [Test]
    public async Task Validate_ShouldPass_WhenNonTeacherUserSelectsValidStepDetail()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var stepDetailId = Guid.NewGuid();

        var step = new Domain.Entities.Models.Step
        {
            Id = stepId,
            Name = "Test Step",
            ShortName = "TS",
            Order = 1
        };
        _dbContext.Steps.Add(step);

        var stepDetail = new Domain.Entities.Models.StepDetail
        {
            Id = stepDetailId,
            StepId = stepId,
            Name = "Test Step Detail",
            Order = 1
        };
        _dbContext.StepDetails.Add(stepDetail);

        var rolePermission = new Domain.Entities.RoleStepPermission
        {
            RoleId = _teacherRoleId,
            StepId = stepId
        };
        _dbContext.RoleStepPermissions.Add(rolePermission);

        _dbContext.SaveChanges();

        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Administrator });
        _identityServiceMock
            .Setup(s => s.GetRoleIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { _teacherRoleId });

        var validator = new GetApplicationsQueryValidator(
            _userMock.Object, _dbContext, _identityServiceMock.Object);

        var query = new GetApplicationsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            StepDetailId = stepDetailId
        };

        // Act
        var result = await validator.ValidateAsync(query, CancellationToken.None);

        // Assert
        result.IsValid.ShouldBeTrue();
    }
}
