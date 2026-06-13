using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RMS.Application.Steps.Commands.CreateStepDetail;
using RMS.Infrastructure.Data;
using Shouldly;
using NotFoundException = Ardalis.GuardClauses.NotFoundException;
using DomainStep = RMS.Domain.Entities.Models.Step;
using DomainStepDetail = RMS.Domain.Entities.Models.StepDetail;

namespace RMS.Application.UnitTests.Steps.Commands.CreateStepDetail;

public class CreateStepDetailCommandHandlerTests : IDisposable
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

    private async Task<DomainStep> CreateStepAsync()
    {
        var step = new DomainStep
        {
            Id = Guid.NewGuid(),
            Name = "Test Step",
            Order = 1
        };

        _dbContext.Steps.Add(step);
        await _dbContext.SaveChangesAsync();

        return step;
    }

    private async Task<DomainStepDetail> CreateStepDetailAsync(Guid stepId)
    {
        var detail = new DomainStepDetail
        {
            Id = Guid.NewGuid(),
            StepId = stepId,
            Name = "Existing Detail",
            Order = 1
        };

        _dbContext.StepDetails.Add(detail);
        await _dbContext.SaveChangesAsync();

        return detail;
    }

    [Test]
    public async Task Handle_ShouldCreateStepDetail_WhenCommandIsValid()
    {
        // Arrange
        var step = await CreateStepAsync();
        var command = new CreateStepDetailCommand
        {
            StepId = step.Id,
            Name = "New Detail",
            Order = 1,
            IsReturnStep = false,
            IsCaculateScoreStep = false
        };

        var handler = new CreateStepDetailCommandHandler(_dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBe(Guid.Empty);

        var detail = await _dbContext.StepDetails.FindAsync(result);
        detail.ShouldNotBeNull();
        detail!.Name.ShouldBe("New Detail");
        detail.StepId.ShouldBe(step.Id);
    }

    [Test]
    public async Task Handle_ShouldCreateStepDetail_WithNextStepDetail_WhenNextStepDetailProvided()
    {
        // Arrange
        var step = await CreateStepAsync();
        var nextDetail = await CreateStepDetailAsync(step.Id);

        var command = new CreateStepDetailCommand
        {
            StepId = step.Id,
            Name = "Detail With Next",
            Order = 2,
            NextStepDetailId = nextDetail.Id
        };

        var handler = new CreateStepDetailCommandHandler(_dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var detail = await _dbContext.StepDetails.FindAsync(result);
        detail!.NextStepDetailId.ShouldBe(nextDetail.Id);
    }

    [Test]
    public async Task Handle_ShouldSetIsReturnStep_WhenProvided()
    {
        // Arrange
        var step = await CreateStepAsync();
        var command = new CreateStepDetailCommand
        {
            StepId = step.Id,
            Name = "Return Detail",
            Order = 1,
            IsReturnStep = true,
            IsCaculateScoreStep = false
        };

        var handler = new CreateStepDetailCommandHandler(_dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var detail = await _dbContext.StepDetails.FindAsync(result);
        detail!.IsReturnStep.ShouldBeTrue();
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenStepDoesNotExist()
    {
        // Arrange
        var command = new CreateStepDetailCommand
        {
            StepId = Guid.NewGuid(),
            Name = "Ghost Detail",
            Order = 1
        };

        var handler = new CreateStepDetailCommandHandler(_dbContext);

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenNextStepDetailDoesNotExist()
    {
        // Arrange
        var step = await CreateStepAsync();
        var command = new CreateStepDetailCommand
        {
            StepId = step.Id,
            Name = "Detail",
            Order = 1,
            NextStepDetailId = Guid.NewGuid()
        };

        var handler = new CreateStepDetailCommandHandler(_dbContext);

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
