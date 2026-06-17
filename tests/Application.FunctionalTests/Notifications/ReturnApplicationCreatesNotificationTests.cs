using Microsoft.EntityFrameworkCore;
using RMS.Application.Application.Commands.ReturnApplication;
using RMS.Domain.Constants;
using RMS.Domain.Entities;
using RMS.Domain.Entities.Models;
using RMS.Domain.Enums;
using RMS.Infrastructure.Data;
using Shouldly;
using DomainApplication = RMS.Domain.Entities.Models.Application;

namespace RMS.Application.FunctionalTests.Notifications;

public class ReturnApplicationCreatesNotificationTests : TestBase
{
    [Test]
    public async Task ShouldCreateReturnedNotification_ForApplicationCreator()
    {
        // Arrange
        var teacherId = await TestApp.RunAsUserAsync("teacher-rt@local", "Testing1234!", [Roles.Teacher]);
        var tttvId = await TestApp.RunAsUserAsync("tttv-rt@local", "Testing1234!", [Roles.Tttv]);

        // Set up Tttv step detail (current step of the application) and a return step detail
        var tttvStepDetail = await CreateStepDetailForRoleAsync(Roles.Tttv, "Tttv step");
        var returnStepDetail = await CreateReturnStepDetailAsync();

        var application = new DomainApplication
        {
            Id = Guid.NewGuid(),
            Code = "APP-RT-001",
            Title = "Application for return",
            Description = "Test app",
            Status = ApplicationStatus.Submitted,
            StepDetailId = tttvStepDetail.Id,
            CreatedBy = teacherId,
        };
        await TestApp.AddAsync(application);

        // Switch to the Tttv user to perform the return
        await TestApp.RunAsUserAsync("tttv-rt@local", "Testing1234!", [Roles.Tttv]);

        var command = new ReturnApplicationCommand
        {
            ApplicationId = application.Id,
            Title = "Thiếu tài liệu",
            Description = "Vui lòng bổ sung bản scan CMND",
        };

        // Act
        await TestApp.SendAsync(command);

        // Assert
        var notifications = await GetNotificationsForUserAsync(teacherId);
        notifications.Count.ShouldBe(1);

        var n = notifications[0];
        n.Type.ShouldBe(NotificationType.ApplicationReturned);
        n.RelatedApplicationId.ShouldBe(application.Id);
        n.Title.ShouldNotBeNullOrWhiteSpace();
        n.Body.ShouldNotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task ShouldSendReturnedNotificationToExplicitRecipient_WhenRecipientIdProvided()
    {
        // Arrange
        var teacherId = await TestApp.RunAsUserAsync("teacher-rt2@local", "Testing1234!", [Roles.Teacher]);
        var customRecipientId = await TestApp.RunAsUserAsync("custom-rt@local", "Testing1234!", [Roles.Administrator]);

        var tttvStepDetail = await CreateStepDetailForRoleAsync(Roles.Tttv, "Tttv step 2");
        await CreateReturnStepDetailAsync();

        var application = new DomainApplication
        {
            Id = Guid.NewGuid(),
            Code = "APP-RT-002",
            Title = "Application for return 2",
            Description = "Test app 2",
            Status = ApplicationStatus.Submitted,
            StepDetailId = tttvStepDetail.Id,
            CreatedBy = teacherId,
        };
        await TestApp.AddAsync(application);

        await TestApp.RunAsUserAsync("tttv-rt2@local", "Testing1234!", [Roles.Tttv]);

        var command = new ReturnApplicationCommand
        {
            ApplicationId = application.Id,
            Title = "Sai định dạng",
            Description = "Vui lòng nộp lại",
            RecipientId = customRecipientId,
        };

        // Act
        await TestApp.SendAsync(command);

        // Assert: teacher does not get the notification; the explicit recipient does.
        var teacherNotifications = await GetNotificationsForUserAsync(teacherId);
        teacherNotifications.ShouldBeEmpty();

        var customNotifications = await GetNotificationsForUserAsync(customRecipientId);
        customNotifications.Count.ShouldBe(1);
    }

    private static async Task<StepDetail> CreateStepDetailForRoleAsync(string roleName, string detailName)
    {
        var step = new Step
        {
            Id = Guid.NewGuid(),
            Name = $"{roleName} step",
            ShortName = roleName,
            Order = 1,
            StepDetails =
            {
                new StepDetail
                {
                    Id = Guid.NewGuid(),
                    Name = detailName,
                    Order = 1,
                }
            }
        };

        await TestApp.AddAsync(step);

        await TestApp.ExecuteDbContextAsync(async context =>
        {
            var role = await context.Roles.SingleAsync(r => r.Name == roleName, CancellationToken.None);
            context.RoleStepPermissions.Add(new RoleStepPermission
            {
                RoleId = role.Id,
                StepId = step.Id,
            });
            await context.SaveChangesAsync();
        });

        return step.StepDetails.Single();
    }

    private static async Task<StepDetail> CreateReturnStepDetailAsync()
    {
        var step = new Step
        {
            Id = Guid.NewGuid(),
            Name = "Return step",
            ShortName = "Return",
            Order = 99,
            StepDetails =
            {
                new StepDetail
                {
                    Id = Guid.NewGuid(),
                    Name = "Return step detail",
                    Order = 1,
                    IsReturnStep = true,
                }
            }
        };

        await TestApp.AddAsync(step);
        return step.StepDetails.Single();
    }

    private static async Task<List<Notification>> GetNotificationsForUserAsync(string userId)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await context.NotificationRecipients
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .Select(r => r.Notification)
            .ToListAsync();
    }
}
