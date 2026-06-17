using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RMS.Application.Common.EventHandlers;
using RMS.Domain.Entities;
using RMS.Domain.Events;
using RMS.Infrastructure.Data;
using Shouldly;

namespace RMS.Application.UnitTests.Common.EventHandlers;

public class ApplicationReturnedEventHandlerTests : IDisposable
{
    private ApplicationDbContext _dbContext = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);
    }

    [TearDown]
    public void TearDown() => _dbContext.Dispose();
    public void Dispose() => _dbContext?.Dispose();

    [Test]
    public async Task Handle_ShouldCreateNotification_WithRecipient()
    {
        var appId = Guid.NewGuid();
        var handler = new ApplicationReturnedEventHandler(_dbContext);

        await handler.Handle(new ApplicationReturnedEvent
        {
            ApplicationId = appId,
            ApplicationCode = "APP-001",
            RecipientId = "teacher-001",
            Title = "Thiếu tài liệu",
            Description = "Vui lòng bổ sung",
        }, CancellationToken.None);

        var notification = await _dbContext.Notifications.SingleAsync();
        notification.Type.ShouldBe(NotificationType.ApplicationReturned);
        notification.RelatedApplicationId.ShouldBe(appId);
        notification.Title.ShouldNotBeNullOrEmpty();
        notification.Body.ShouldNotBeNullOrEmpty();

        var recipient = await _dbContext.NotificationRecipients.SingleAsync();
        recipient.UserId.ShouldBe("teacher-001");
        recipient.IsRead.ShouldBeFalse();
        recipient.NotificationId.ShouldBe(notification.Id);
    }

    [Test]
    public async Task Handle_ShouldSkip_WhenRecipientIdIsEmpty()
    {
        var handler = new ApplicationReturnedEventHandler(_dbContext);

        await handler.Handle(new ApplicationReturnedEvent
        {
            ApplicationId = Guid.NewGuid(),
            ApplicationCode = "APP-001",
            RecipientId = string.Empty,
            Title = "t",
            Description = "d",
        }, CancellationToken.None);

        (await _dbContext.Notifications.CountAsync()).ShouldBe(0);
    }
}
