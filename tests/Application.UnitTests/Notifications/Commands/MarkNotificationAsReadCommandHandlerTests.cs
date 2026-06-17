using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RMS.Application.Common.Exceptions;
using RMS.Application.Common.Interfaces;
using RMS.Application.Notifications.Commands.MarkNotificationAsRead;
using RMS.Domain.Entities;
using RMS.Infrastructure.Data;
using Shouldly;
using NotFoundException = Ardalis.GuardClauses.NotFoundException;

namespace RMS.Application.UnitTests.Notifications.Commands;

public class MarkNotificationAsReadCommandHandlerTests : IDisposable
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<IUser> _userMock = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);
        _userMock = new Mock<IUser>();
    }

    [TearDown]
    public void TearDown() => _dbContext.Dispose();
    public void Dispose() => _dbContext?.Dispose();

    [Test]
    public async Task Handle_ShouldThrowNotFound_WhenRecipientMissing()
    {
        _userMock.Setup(u => u.Id).Returns("user-1");
        var handler = new MarkNotificationAsReadCommandHandler(_dbContext, _userMock.Object);

        await Should.ThrowAsync<NotFoundException>(() =>
            handler.Handle(new MarkNotificationAsReadCommand { Id = Guid.NewGuid() }, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ShouldThrowForbidden_WhenUserIdEmpty()
    {
        _userMock.Setup(u => u.Id).Returns((string?)null);
        var handler = new MarkNotificationAsReadCommandHandler(_dbContext, _userMock.Object);

        await Should.ThrowAsync<ForbiddenAccessException>(() =>
            handler.Handle(new MarkNotificationAsReadCommand { Id = Guid.NewGuid() }, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ShouldMarkAsRead_ForRecipient()
    {
        var n = new Notification
        {
            Id = Guid.NewGuid(),
            Title = "t",
            Body = "b",
            Type = NotificationType.ApplicationReturned,
        };
        _dbContext.Notifications.Add(n);
        _dbContext.NotificationRecipients.Add(new NotificationRecipient
        {
            NotificationId = n.Id,
            UserId = "user-1",
            IsRead = false,
        });
        await _dbContext.SaveChangesAsync();

        _userMock.Setup(u => u.Id).Returns("user-1");
        var handler = new MarkNotificationAsReadCommandHandler(_dbContext, _userMock.Object);

        await handler.Handle(new MarkNotificationAsReadCommand { Id = n.Id }, CancellationToken.None);

        var updated = await _dbContext.NotificationRecipients
            .FirstAsync(r => r.NotificationId == n.Id && r.UserId == "user-1");
        updated.IsRead.ShouldBeTrue();
        updated.ReadAt.ShouldNotBeNull();
    }

    [Test]
    public async Task Handle_ShouldNotThrow_WhenAlreadyRead()
    {
        var n = new Notification
        {
            Id = Guid.NewGuid(),
            Title = "t",
            Body = "b",
            Type = NotificationType.ApplicationReturned,
        };
        _dbContext.Notifications.Add(n);
        _dbContext.NotificationRecipients.Add(new NotificationRecipient
        {
            NotificationId = n.Id,
            UserId = "user-1",
            IsRead = true,
            ReadAt = DateTimeOffset.UtcNow.AddDays(-1),
        });
        await _dbContext.SaveChangesAsync();

        _userMock.Setup(u => u.Id).Returns("user-1");
        var handler = new MarkNotificationAsReadCommandHandler(_dbContext, _userMock.Object);

        await handler.Handle(new MarkNotificationAsReadCommand { Id = n.Id }, CancellationToken.None);

        var updated = await _dbContext.NotificationRecipients
            .FirstAsync(r => r.NotificationId == n.Id && r.UserId == "user-1");
        updated.IsRead.ShouldBeTrue();
    }
}
