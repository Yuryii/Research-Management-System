using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RMS.Application.Common.Interfaces;
using RMS.Application.Common.Options;
using RMS.Application.Steps.Queries;
using RMS.Domain.Constants;
using Shouldly;

namespace RMS.Application.UnitTests.Steps.Queries.GetDefaultStepIdForUser;

public class GetDefaultStepIdForUserQueryHandlerTests
{
    private Mock<IUser> _userMock = null!;
    private IOptions<DefaultStepIdsOptions> _options = null!;

    private readonly Guid _teacherStepId = Guid.NewGuid();
    private readonly Guid _dvqlttStepId = Guid.NewGuid();
    private readonly Guid _tttvStepId = Guid.NewGuid();
    private readonly Guid _khcnHtqtStepId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _userMock = new Mock<IUser>();
        _options = Options.Create(new DefaultStepIdsOptions
        {
            TeacherStepId = _teacherStepId,
            DvqlttStepId = _dvqlttStepId,
            TttvStepId = _tttvStepId,
            KhcnHtqtStepId = _khcnHtqtStepId
        });
    }

    private GetDefaultStepIdForUserQueryHandler CreateHandler()
    {
        return new GetDefaultStepIdForUserQueryHandler(_userMock.Object, _options);
    }

    [Test]
    public async Task Handle_ShouldReturnTeacherStepId_WhenUserHasTeacherRole()
    {
        // Arrange
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Teacher });

        var handler = CreateHandler();
        var query = new GetDefaultStepIdForUserQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBe(_teacherStepId);
    }

    [Test]
    public async Task Handle_ShouldReturnDvqlttStepId_WhenUserHasDvqlttRole()
    {
        // Arrange
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Dvqltt });

        var handler = CreateHandler();
        var query = new GetDefaultStepIdForUserQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBe(_dvqlttStepId);
    }

    [Test]
    public async Task Handle_ShouldReturnTttvStepId_WhenUserHasTttvRole()
    {
        // Arrange
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Tttv });

        var handler = CreateHandler();
        var query = new GetDefaultStepIdForUserQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBe(_tttvStepId);
    }

    [Test]
    public async Task Handle_ShouldReturnKhcnHtqtStepId_WhenUserHasKhcnHtqtRole()
    {
        // Arrange
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.KhcnHtqt });

        var handler = CreateHandler();
        var query = new GetDefaultStepIdForUserQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBe(_khcnHtqtStepId);
    }

    [Test]
    public async Task Handle_ShouldReturnDvqlttStepId_WhenUserIsAdministrator_AndDvqlttStepIdIsSet()
    {
        // Arrange
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Administrator });

        var handler = CreateHandler();
        var query = new GetDefaultStepIdForUserQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBe(_dvqlttStepId);
    }

    [Test]
    public async Task Handle_ShouldReturnTeacherStepId_WhenUserIsAdministrator_AndDvqlttStepIdIsEmpty_ButTeacherStepIdIsSet()
    {
        // Arrange
        var options = Options.Create(new DefaultStepIdsOptions
        {
            TeacherStepId = _teacherStepId,
            DvqlttStepId = Guid.Empty,
            TttvStepId = _tttvStepId,
            KhcnHtqtStepId = _khcnHtqtStepId
        });

        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Administrator });
        var handler = new GetDefaultStepIdForUserQueryHandler(_userMock.Object, options);
        var query = new GetDefaultStepIdForUserQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBe(_teacherStepId);
    }

    [Test]
    public async Task Handle_ShouldReturnEmptyGuid_WhenUserIsAdministrator_AndBothStepIdsAreEmpty()
    {
        // Arrange
        var options = Options.Create(new DefaultStepIdsOptions
        {
            TeacherStepId = Guid.Empty,
            DvqlttStepId = Guid.Empty,
            TttvStepId = Guid.Empty,
            KhcnHtqtStepId = Guid.Empty
        });

        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Administrator });
        var handler = new GetDefaultStepIdForUserQueryHandler(_userMock.Object, options);
        var query = new GetDefaultStepIdForUserQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBe(Guid.Empty);
    }

    [Test]
    public async Task Handle_ShouldReturnEmptyGuid_WhenUserHasNoRoles()
    {
        // Arrange
        _userMock.Setup(u => u.Roles).Returns((List<string>?)null);

        var handler = CreateHandler();
        var query = new GetDefaultStepIdForUserQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBe(Guid.Empty);
    }

    [Test]
    public async Task Handle_ShouldReturnEmptyGuid_WhenUserHasEmptyRoles()
    {
        // Arrange
        _userMock.Setup(u => u.Roles).Returns(new List<string>());

        var handler = CreateHandler();
        var query = new GetDefaultStepIdForUserQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBe(Guid.Empty);
    }

    [Test]
    public async Task Handle_ShouldReturnEmptyGuid_WhenUserHasUnknownRole()
    {
        // Arrange
        _userMock.Setup(u => u.Roles).Returns(new List<string> { "UnknownRole" });

        var handler = CreateHandler();
        var query = new GetDefaultStepIdForUserQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBe(Guid.Empty);
    }

    [Test]
    public async Task Handle_ShouldReturnFirstMatchingRoleStepId_WhenUserHasMultipleRoles()
    {
        // Arrange — Teacher role is first in the if-chain
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Teacher, Roles.Dvqltt, Roles.Tttv });

        var handler = CreateHandler();
        var query = new GetDefaultStepIdForUserQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert — Teacher is checked first, so TeacherStepId is returned
        result.ShouldBe(_teacherStepId);
    }
}
