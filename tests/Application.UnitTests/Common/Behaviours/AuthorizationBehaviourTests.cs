using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RMS.Application.Application.Commands.CreateApplication;
using RMS.Application.Common.Behaviours;
using RMS.Application.Common.Interfaces;
using RMS.Application.Common.Security;
using Shouldly;

namespace RMS.Application.UnitTests.Common.Behaviours;

public class AuthorizationBehaviourTests
{
    private Mock<IUser> _userMock = null!;
    private Mock<IIdentityService> _identityServiceMock = null!;

    [SetUp]
    public void SetUp()
    {
        _userMock = new Mock<IUser>();
        _identityServiceMock = new Mock<IIdentityService>();
    }

    [Test]
    public async Task Handle_RequestWithoutAuthorizeAttribute_PassesThrough()
    {
        // Arrange
        var sut = new AuthorizationBehaviour<NoAuthorizeTestCommand, Guid>(
            _userMock.Object,
            _identityServiceMock.Object);

        var request = new NoAuthorizeTestCommand();
        var response = Guid.NewGuid();

        RequestHandlerDelegate<Guid> next = (ct) => Task.FromResult(response);

        // Act
        var result = await sut.Handle(request, next, CancellationToken.None);

        // Assert
        result.ShouldBe(response);
        _userMock.Verify(u => u.Id, Times.Never);
    }

    [Test]
    public async Task Handle_RequestWithAuthorizeAttribute_ThrowsUnauthorizedAccessException_WhenUserIdIsNull()
    {
        // Arrange
        _userMock.Setup(u => u.Id).Returns((string?)null);

        var sut = new AuthorizationBehaviour<AuthorizeTestCommand, Guid>(
            _userMock.Object,
            _identityServiceMock.Object);

        var request = new AuthorizeTestCommand();
        RequestHandlerDelegate<Guid> next = (ct) => Task.FromResult(Guid.NewGuid());

        // Act & Assert
        await Should.ThrowAsync<UnauthorizedAccessException>(() => sut.Handle(request, next, CancellationToken.None));
    }

    [Test]
    public async Task Handle_RequestWithAuthorizeAttributeAndRoles_ThrowsForbiddenAccessException_WhenUserHasNoMatchingRole()
    {
        // Arrange
        _userMock.Setup(u => u.Id).Returns("user-123");
        _userMock.Setup(u => u.Roles).Returns(new List<string> { "Student" });

        var sut = new AuthorizationBehaviour<AuthorizeWithRolesTestCommand, Guid>(
            _userMock.Object,
            _identityServiceMock.Object);

        var request = new AuthorizeWithRolesTestCommand();
        RequestHandlerDelegate<Guid> next = (ct) => Task.FromResult(Guid.NewGuid());

        // Act & Assert
        await Should.ThrowAsync<RMS.Application.Common.Exceptions.ForbiddenAccessException>(
            () => sut.Handle(request, next, CancellationToken.None));
    }

    [Test]
    public async Task Handle_RequestWithAuthorizeAttributeAndRoles_Succeeds_WhenUserHasMatchingRole()
    {
        // Arrange
        _userMock.Setup(u => u.Id).Returns("user-123");
        _userMock.Setup(u => u.Roles).Returns(new List<string> { "Teacher" });

        var sut = new AuthorizationBehaviour<AuthorizeWithRolesTestCommand, Guid>(
            _userMock.Object,
            _identityServiceMock.Object);

        var request = new AuthorizeWithRolesTestCommand();
        var expectedResponse = Guid.NewGuid();
        RequestHandlerDelegate<Guid> next = (ct) => Task.FromResult(expectedResponse);

        // Act
        var result = await sut.Handle(request, next, CancellationToken.None);

        // Assert
        result.ShouldBe(expectedResponse);
    }

    [Test]
    public async Task Handle_RequestWithAuthorizeAttributeAndMultipleRoles_Succeeds_WhenUserHasOneMatchingRole()
    {
        // Arrange
        _userMock.Setup(u => u.Id).Returns("user-123");
        _userMock.Setup(u => u.Roles).Returns(new List<string> { "Teacher", "Admin" });

        var sut = new AuthorizationBehaviour<AuthorizeWithMultipleRolesTestCommand, Guid>(
            _userMock.Object,
            _identityServiceMock.Object);

        var request = new AuthorizeWithMultipleRolesTestCommand();
        var expectedResponse = Guid.NewGuid();
        RequestHandlerDelegate<Guid> next = (ct) => Task.FromResult(expectedResponse);

        // Act
        var result = await sut.Handle(request, next, CancellationToken.None);

        // Assert
        result.ShouldBe(expectedResponse);
    }

    [Test]
    public async Task Handle_RequestWithAuthorizeAttributeAndPolicy_ThrowsForbiddenAccessException_WhenPolicyCheckFails()
    {
        // Arrange
        _userMock.Setup(u => u.Id).Returns("user-123");
        _userMock.Setup(u => u.Roles).Returns((List<string>?)null);

        _identityServiceMock
            .Setup(s => s.AuthorizeAsync("user-123", "TestPolicy"))
            .ReturnsAsync(false);

        var sut = new AuthorizationBehaviour<AuthorizeWithPolicyTestCommand, Guid>(
            _userMock.Object,
            _identityServiceMock.Object);

        var request = new AuthorizeWithPolicyTestCommand();
        RequestHandlerDelegate<Guid> next = (ct) => Task.FromResult(Guid.NewGuid());

        // Act & Assert
        await Should.ThrowAsync<RMS.Application.Common.Exceptions.ForbiddenAccessException>(
            () => sut.Handle(request, next, CancellationToken.None));
    }

    [Test]
    public async Task Handle_RequestWithAuthorizeAttributeAndPolicy_Succeeds_WhenPolicyCheckPasses()
    {
        // Arrange
        _userMock.Setup(u => u.Id).Returns("user-123");
        _userMock.Setup(u => u.Roles).Returns((List<string>?)null);

        _identityServiceMock
            .Setup(s => s.AuthorizeAsync("user-123", "TestPolicy"))
            .ReturnsAsync(true);

        var sut = new AuthorizationBehaviour<AuthorizeWithPolicyTestCommand, Guid>(
            _userMock.Object,
            _identityServiceMock.Object);

        var request = new AuthorizeWithPolicyTestCommand();
        var expectedResponse = Guid.NewGuid();
        RequestHandlerDelegate<Guid> next = (ct) => Task.FromResult(expectedResponse);

        // Act
        var result = await sut.Handle(request, next, CancellationToken.None);

        // Assert
        result.ShouldBe(expectedResponse);
    }

    // Test request classes - NO [Authorize] attribute (checked via reflection on request type)
    private class NoAuthorizeTestCommand : IRequest<Guid>
    {
    }

    [Authorize]
    private class AuthorizeTestCommand : IRequest<Guid>
    {
    }

    [Authorize(Roles = "Teacher,Administrator")]
    private class AuthorizeWithRolesTestCommand : IRequest<Guid>
    {
    }

    [Authorize(Roles = "Teacher,Administrator")]
    private class AuthorizeWithMultipleRolesTestCommand : IRequest<Guid>
    {
    }

    [Authorize(Policy = "TestPolicy")]
    private class AuthorizeWithPolicyTestCommand : IRequest<Guid>
    {
    }
}
