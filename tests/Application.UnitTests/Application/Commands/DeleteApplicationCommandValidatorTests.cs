using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RMS.Application.Application.Commands.DeleteApplication;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Entities.Models;
using RMS.Domain.Enums;
using FluentValidation.TestHelper;
using DomainApplication = RMS.Domain.Entities.Models.Application;

namespace RMS.Application.UnitTests.Application.Commands;

public class DeleteApplicationCommandValidatorTests
{
    private DeleteApplicationCommandValidator CreateValidator(IApplicationDbContext? context = null)
    {
        return new DeleteApplicationCommandValidator(context!);
    }

    private static IApplicationDbContext CreateMockDbContext(Guid? appId, ApplicationStatus? status)
    {
        var mockContext = new Mock<IApplicationDbContext>();
        var mockDbSet = new Mock<DbSet<DomainApplication>>();

        if (appId.HasValue)
        {
            var application = new DomainApplication
            {
                Id = appId.Value,
                Code = "APP-001",
                Title = "Test Application",
                Description = "Test Description",
                Status = status ?? ApplicationStatus.Draft
            };

            mockDbSet.Setup(d => d.FindAsync(new object[] { appId.Value }, It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult<DomainApplication?>(application));
        }
        else
        {
            mockDbSet.Setup(d => d.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult<DomainApplication?>(null));
        }

        mockContext.Setup(c => c.Applications).Returns(mockDbSet.Object);
        return mockContext.Object;
    }

    [Test]
    public async Task Validate_ShouldFail_WhenIdIsEmpty()
    {
        var validator = CreateValidator();

        var command = new DeleteApplicationCommand(Guid.Empty);

        var result = await validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Application Id is required.");
    }

    [Test]
    public async Task Validate_ShouldFail_WhenApplicationNotFound()
    {
        var context = CreateMockDbContext(appId: null, status: null);
        var validator = CreateValidator(context);

        var command = new DeleteApplicationCommand(Guid.NewGuid());

        var result = await validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Only applications in Draft status can be deleted.");
    }

    [Test]
    public async Task Validate_ShouldFail_WhenApplicationIsNotDraft()
    {
        var appId = Guid.NewGuid();
        var context = CreateMockDbContext(appId, ApplicationStatus.Submitted);
        var validator = CreateValidator(context);

        var command = new DeleteApplicationCommand(appId);

        var result = await validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Only applications in Draft status can be deleted.");
    }

    [Test]
    public async Task Validate_ShouldPass_WhenApplicationIsDraft()
    {
        var appId = Guid.NewGuid();
        var context = CreateMockDbContext(appId, ApplicationStatus.Draft);
        var validator = CreateValidator(context);

        var command = new DeleteApplicationCommand(appId);

        var result = await validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
