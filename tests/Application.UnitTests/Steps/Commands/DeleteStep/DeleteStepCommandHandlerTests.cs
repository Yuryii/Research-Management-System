using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RMS.Application.Steps.Commands.DeleteStep;
using RMS.Infrastructure.Data;
using Shouldly;
using NotFoundException = Ardalis.GuardClauses.NotFoundException;
using DomainStep = RMS.Domain.Entities.Models.Step;
using DomainStepDetail = RMS.Domain.Entities.Models.StepDetail;

namespace RMS.Application.UnitTests.Steps.Commands.DeleteStep;

public class DeleteStepCommandHandlerTests : IDisposable
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

    [Test]
    public async Task Handle_ShouldDeleteStep_WhenStepExists()
    {
        // Arrange
        var step = new DomainStep
        {
            Id = Guid.NewGuid(),
            Name = "To Delete",
            Order = 1
        };

        _dbContext.Steps.Add(step);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteStepCommand(step.Id);
        var handler = new DeleteStepCommandHandler(_dbContext);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var count = await _dbContext.Steps.CountAsync();
        count.ShouldBe(0);
    }

    [Test]
    public async Task Handle_ShouldCascadeDeleteStepDetails_WhenStepHasDetails()
    {
        // Arrange
        var step = new DomainStep
        {
            Id = Guid.NewGuid(),
            Name = "Step With Details",
            Order = 1
        };

        var detail1 = new DomainStepDetail
        {
            Id = Guid.NewGuid(),
            StepId = step.Id,
            Name = "Detail 1",
            Order = 1
        };

        var detail2 = new DomainStepDetail
        {
            Id = Guid.NewGuid(),
            StepId = step.Id,
            Name = "Detail 2",
            Order = 2
        };

        _dbContext.Steps.Add(step);
        _dbContext.StepDetails.Add(detail1);
        _dbContext.StepDetails.Add(detail2);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteStepCommand(step.Id);
        var handler = new DeleteStepCommandHandler(_dbContext);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        (await _dbContext.Steps.CountAsync()).ShouldBe(0);
        (await _dbContext.StepDetails.CountAsync()).ShouldBe(0);
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenStepDoesNotExist()
    {
        // Arrange
        var command = new DeleteStepCommand(Guid.NewGuid());
        var handler = new DeleteStepCommandHandler(_dbContext);

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
