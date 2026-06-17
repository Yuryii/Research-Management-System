using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RMS.Application.Common.Interfaces;
using RMS.Application.Notifications.Commands.MarkAllNotificationsAsRead;
using RMS.Domain.Entities;
using RMS.Infrastructure.Data;
using Shouldly;

namespace RMS.Application.UnitTests.Notifications.Commands;

public class MarkAllNotificationsAsReadCommandHandlerTests : IDisposable
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
    public async Task Handle_ShouldMarkAllUnreadAsRead_ForCurrentUser()
    {
        var n1 = new Notification
        {
            Id = Guid.NewGuid(),
            Title = "a",
            Body = "a",
            Type = NotificationType.ApplicationReturned,
        };
        var n2 = new Notification
        {
            Id = Guid.NewGuid(),
            Title = "b",
            Body = "b",
            Type = NotificationType.ApplicationForwarded,
        };
        var n3 = new Notification
        {
            Id = Guid.NewGuid(),
            Title = "c",
            Body = "c",
            Type = NotificationType.ApplicationApproved,
        };

        _dbContext.Notifications.AddRange(n1, n2, n3);

        _dbContext.NotificationRecipients.Add(new NotificationRecipient
        {
            NotificationId = n1.Id,
            UserId = "u",
            IsRead = false,
        });
        _dbContext.NotificationRecipients.Add(new NotificationRecipient
        {
            NotificationId = n2.Id,
            UserId = "u",
            IsRead = false,
        });
        _dbContext.NotificationRecipients.Add(new NotificationRecipient
        {
            NotificationId = n3.Id,
            UserId = "other",
            IsRead = false,
        });

        await _dbContext.SaveChangesAsync();

        _userMock.Setup(u => u.Id).Returns("u");
        var handler = new MarkAllNotificationsAsReadCommandHandler(_dbContext, _userMock.Object);

        var updated = await handler.Handle(new MarkAllNotificationsAsReadCommand(), CancellationToken.None);

        updated.ShouldBe(2);
        var all = await _dbContext.NotificationRecipients.ToListAsync();
        all.Where(r => r.UserId == "u").All(r => r.IsRead).ShouldBeTrue();
        all.Where(r => r.UserId == "other").All(r => !r.IsRead).ShouldBeTrue();
    }

    [Test]
    public async Task Handle_ShouldReturnZero_WhenNoUnread()
    {
        _userMock.Setup(u => u.Id).Returns("u");
        var handler = new MarkAllNotificationsAsReadCommandHandler(_dbContext, _userMock.Object);

        var updated = await handler.Handle(new MarkAllNotificationsAsReadCommand(), CancellationToken.None);

        updated.ShouldBe(0);
    }
}
