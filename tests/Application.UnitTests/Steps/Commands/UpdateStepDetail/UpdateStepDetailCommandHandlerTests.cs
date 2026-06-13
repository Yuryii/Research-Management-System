using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RMS.Application.Steps.Commands.UpdateStepDetail;
using RMS.Infrastructure.Data;
using Shouldly;
using NotFoundException = Ardalis.GuardClauses.NotFoundException;
using DomainStepDetail = RMS.Domain.Entities.Models.StepDetail;

namespace RMS.Application.UnitTests.Steps.Commands.UpdateStepDetail;

public class UpdateStepDetailCommandHandlerTests : IDisposable
{
    private ApplicationDbContext _dbContext = null!;

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

    private async Task<DomainStepDetail> CreateStepDetailAsync(
        Guid stepId,
        string name = "Original Detail",
        int order = 1,
        bool isReturnStep = false,
        bool isCalculateScore = false)
    {
        var detail = new DomainStepDetail
        {
            Id = Guid.NewGuid(),
            StepId = stepId,
            Name = name,
            Order = order,
            IsReturnStep = isReturnStep,
            IsCaculateScoreStep = isCalculateScore
        };

        _dbContext.StepDetails.Add(detail);
        await _dbContext.SaveChangesAsync();

        return detail;
    }

    private async Task<DomainStepDetail> CreateStepAsync(Guid stepId)
    {
        var step = new RMS.Domain.Entities.Models.Step
        {
            Id = stepId,
            Name = "Test Step",
            Order = 1
        };
        _dbContext.Steps.Add(step);
        await _dbContext.SaveChangesAsync();
        return await CreateStepDetailAsync(stepId);
    }

    [Test]
    public async Task Handle_ShouldUpdateName_WhenNameProvided()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var detail = await CreateStepAsync(stepId);
        var command = new UpdateStepDetailCommand { Id = detail.Id, Name = "Updated Name" };
        var handler = new UpdateStepDetailCommandHandler(_dbContext);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _dbContext.StepDetails.FindAsync(detail.Id);
        updated!.Name.ShouldBe("Updated Name");
    }

    [Test]
    public async Task Handle_ShouldUpdateOrder_WhenOrderProvided()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var detail = await CreateStepAsync(stepId);
        var command = new UpdateStepDetailCommand { Id = detail.Id, Order = 99 };
        var handler = new UpdateStepDetailCommandHandler(_dbContext);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _dbContext.StepDetails.FindAsync(detail.Id);
        updated!.Order.ShouldBe(99);
    }

    [Test]
    public async Task Handle_ShouldUpdateNextStepDetail_WhenNextStepDetailProvided()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var detail = await CreateStepAsync(stepId);
        var nextDetail = await CreateStepDetailAsync(stepId, "Next Detail", 2);

        var command = new UpdateStepDetailCommand { Id = detail.Id, NextStepDetailId = nextDetail.Id };
        var handler = new UpdateStepDetailCommandHandler(_dbContext);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _dbContext.StepDetails.FindAsync(detail.Id);
        updated!.NextStepDetailId.ShouldBe(nextDetail.Id);
    }

    [Test]
    public async Task Handle_ShouldUpdateIsReturnStep_WhenProvided()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var detail = await CreateStepAsync(stepId);
        var command = new UpdateStepDetailCommand { Id = detail.Id, IsReturnStep = true };
        var handler = new UpdateStepDetailCommandHandler(_dbContext);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _dbContext.StepDetails.FindAsync(detail.Id);
        updated!.IsReturnStep.ShouldBeTrue();
    }

    [Test]
    public async Task Handle_ShouldUpdateMultipleFields_WhenAllProvided()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var detail = await CreateStepAsync(stepId);
        var nextDetail = await CreateStepDetailAsync(stepId, "Next", 2);

        var command = new UpdateStepDetailCommand
        {
            Id = detail.Id,
            Name = "All Updated",
            Order = 50,
            NextStepDetailId = nextDetail.Id,
            IsReturnStep = true,
            IsCaculateScoreStep = true
        };
        var handler = new UpdateStepDetailCommandHandler(_dbContext);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _dbContext.StepDetails.FindAsync(detail.Id);
        updated!.Name.ShouldBe("All Updated");
        updated.Order.ShouldBe(50);
        updated.NextStepDetailId.ShouldBe(nextDetail.Id);
        updated.IsReturnStep.ShouldBeTrue();
        updated.IsCaculateScoreStep.ShouldBeTrue();
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenStepDetailDoesNotExist()
    {
        // Arrange
        var command = new UpdateStepDetailCommand { Id = Guid.NewGuid(), Name = "Ghost" };
        var handler = new UpdateStepDetailCommandHandler(_dbContext);

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
