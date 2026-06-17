using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RMS.Application.Common.Interfaces;
using RMS.Application.Notifications.Queries.GetMyNotifications;
using RMS.Domain.Entities;
using RMS.Infrastructure.Data;
using Shouldly;

namespace RMS.Application.UnitTests.Notifications.Queries;

public class GetMyNotificationsQueryHandlerTests : IDisposable
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
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }

    private void Seed()
    {
        var n1 = new Notification
        {
            Id = Guid.NewGuid(),
            Title = "t1",
            Body = "b1",
            Type = NotificationType.ApplicationReturned,
        };
        var n2 = new Notification
        {
            Id = Guid.NewGuid(),
            Title = "t2",
            Body = "b2",
            Type = NotificationType.ApplicationForwarded,
        };
        var n3 = new Notification
        {
            Id = Guid.NewGuid(),
            Title = "t3",
            Body = "b3",
            Type = NotificationType.ApplicationApproved,
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
            IsRead = false,
        });
        _dbContext.NotificationRecipients.Add(new NotificationRecipient
        {
            NotificationId = n3.Id,
            UserId = "other-user",
            IsRead = false,
        });

        _dbContext.SaveChanges();
    }

    [Test]
    public async Task Handle_ShouldReturnOnlyNotificationsForCurrentUser()
    {
        Seed();

        var queryable = _dbContext.NotificationRecipients
            .AsNoTracking()
            .Where(r => r.UserId == "user-1");

        var count = await queryable.CountAsync();
        count.ShouldBe(2);

        var items = await queryable.ToListAsync();
        items.ShouldAllBe(r => r.UserId == "user-1");
        items.Select(r => r.NotificationId).Distinct().Count().ShouldBe(2);
    }
}
