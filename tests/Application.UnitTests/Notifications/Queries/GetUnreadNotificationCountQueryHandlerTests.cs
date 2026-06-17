using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RMS.Application.Common.Interfaces;
using RMS.Application.Notifications.Queries.GetUnreadNotificationCount;
using RMS.Domain.Entities;
using RMS.Infrastructure.Data;
using Shouldly;

namespace RMS.Application.UnitTests.Notifications.Queries;

public class GetUnreadNotificationCountQueryHandlerTests : IDisposable
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
    public async Task Handle_ShouldReturnZero_WhenUserIdEmpty()
    {
        _userMock.Setup(u => u.Id).Returns((string?)null);
        var handler = new GetUnreadNotificationCountQueryHandler(_dbContext, _userMock.Object);

        var count = await handler.Handle(new GetUnreadNotificationCountQuery(), CancellationToken.None);

        count.ShouldBe(0);
    }

    [Test]
    public async Task Handle_ShouldCountOnlyUnreadForCurrentUser()
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
            Type = NotificationType.ApplicationReturned,
        };
        var n3 = new Notification
        {
            Id = Guid.NewGuid(),
            Title = "c",
            Body = "c",
            Type = NotificationType.ApplicationReturned,
        };

        _dbContext.Notifications.AddRange(n1, n2, n3);

        _dbContext.NotificationRecipients.Add(new NotificationRecipient
        {
            NotificationId = n1.Id,
            UserId = "user-1",
            IsRead = false,
        });
        _dbContext.NotificationRecipients.Add(new NotificationRecipient
        {
            NotificationId = n2.Id,
            UserId = "user-1",
            IsRead = true,
        });
        _dbContext.NotificationRecipients.Add(new NotificationRecipient
        {
            NotificationId = n3.Id,
            UserId = "other",
            IsRead = false,
        });

        await _dbContext.SaveChangesAsync();

        _userMock.Setup(u => u.Id).Returns("user-1");
        var handler = new GetUnreadNotificationCountQueryHandler(_dbContext, _userMock.Object);

        var count = await handler.Handle(new GetUnreadNotificationCountQuery(), CancellationToken.None);

        count.ShouldBe(1);
    }
}
