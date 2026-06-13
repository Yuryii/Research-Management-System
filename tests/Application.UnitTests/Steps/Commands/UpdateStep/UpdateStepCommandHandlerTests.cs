using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RMS.Application.Steps.Commands.UpdateStep;
using RMS.Infrastructure.Data;
using Shouldly;
using NotFoundException = Ardalis.GuardClauses.NotFoundException;
using DomainStep = RMS.Domain.Entities.Models.Step;

namespace RMS.Application.UnitTests.Steps.Commands.UpdateStep;

public class UpdateStepCommandHandlerTests : IDisposable
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

    private async Task<DomainStep> CreateStepAsync(string name = "Original Step", string shortName = "OS", int order = 1)
    {
        var step = new DomainStep
        {
            Id = Guid.NewGuid(),
            Name = name,
            ShortName = shortName,
            Order = order
        };

        _dbContext.Steps.Add(step);
        await _dbContext.SaveChangesAsync();

        return step;
    }

    [Test]
    public async Task Handle_ShouldUpdateName_WhenNameProvided()
    {
        // Arrange
        var step = await CreateStepAsync();
        var command = new UpdateStepCommand { Id = step.Id, Name = "Updated Name" };
        var handler = new UpdateStepCommandHandler(_dbContext);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _dbContext.Steps.FindAsync(step.Id);
        updated!.Name.ShouldBe("Updated Name");
    }

    [Test]
    public async Task Handle_ShouldUpdateShortName_WhenShortNameProvided()
    {
        // Arrange
        var step = await CreateStepAsync();
        var command = new UpdateStepCommand { Id = step.Id, ShortName = "UN" };
        var handler = new UpdateStepCommandHandler(_dbContext);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _dbContext.Steps.FindAsync(step.Id);
        updated!.ShortName.ShouldBe("UN");
    }

    [Test]
    public async Task Handle_ShouldUpdateOrder_WhenOrderProvided()
    {
        // Arrange
        var step = await CreateStepAsync();
        var command = new UpdateStepCommand { Id = step.Id, Order = 99 };
        var handler = new UpdateStepCommandHandler(_dbContext);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _dbContext.Steps.FindAsync(step.Id);
        updated!.Order.ShouldBe(99);
    }

    [Test]
    public async Task Handle_ShouldUpdateMultipleFields_WhenAllProvided()
    {
        // Arrange
        var step = await CreateStepAsync();
        var command = new UpdateStepCommand
        {
            Id = step.Id,
            Name = "All Updated",
            ShortName = "AU",
            Order = 42
        };
        var handler = new UpdateStepCommandHandler(_dbContext);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _dbContext.Steps.FindAsync(step.Id);
        updated!.Name.ShouldBe("All Updated");
        updated.ShortName.ShouldBe("AU");
        updated.Order.ShouldBe(42);
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenStepDoesNotExist()
    {
        // Arrange
        var command = new UpdateStepCommand { Id = Guid.NewGuid(), Name = "Ghost Step" };
        var handler = new UpdateStepCommandHandler(_dbContext);

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
