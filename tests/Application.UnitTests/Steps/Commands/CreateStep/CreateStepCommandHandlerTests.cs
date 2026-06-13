using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RMS.Application.Steps.Commands.CreateStep;
using RMS.Infrastructure.Data;
using Shouldly;
using DomainStep = RMS.Domain.Entities.Models.Step;

namespace RMS.Application.UnitTests.Steps.Commands.CreateStep;

public class CreateStepCommandHandlerTests : IDisposable
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
    public async Task Handle_ShouldCreateStep_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateStepCommand
        {
            Name = "New Step",
            ShortName = "NS",
            Order = 1
        };

        var handler = new CreateStepCommandHandler(_dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBe(Guid.Empty);

        var step = await _dbContext.Steps.FindAsync(result);
        step.ShouldNotBeNull();
        step.Name.ShouldBe("New Step");
    }

    [Test]
    public async Task Handle_ShouldSetNameAndShortNameAndOrder()
    {
        // Arrange
        var command = new CreateStepCommand
        {
            Name = "Approval Step",
            ShortName = "AP",
            Order = 5
        };

        var handler = new CreateStepCommandHandler(_dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var step = await _dbContext.Steps.FindAsync(result);
        step!.Name.ShouldBe("Approval Step");
        step.ShortName.ShouldBe("AP");
        step.Order.ShouldBe(5);
    }

    [Test]
    public async Task Handle_ShouldSaveChangesOnce()
    {
        // Arrange
        var command = new CreateStepCommand
        {
            Name = "Test Step",
            Order = 1
        };

        var handler = new CreateStepCommandHandler(_dbContext);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var count = await _dbContext.Steps.CountAsync();
        count.ShouldBe(1);
    }

    [Test]
    public async Task Handle_ShouldGenerateNewGuid()
    {
        // Arrange
        var command = new CreateStepCommand
        {
            Name = "Test Step",
            Order = 0
        };

        var handler = new CreateStepCommandHandler(_dbContext);

        // Act
        var result1 = await handler.Handle(command, CancellationToken.None);
        var result2 = await handler.Handle(command, CancellationToken.None);

        // Assert
        result1.ShouldNotBe(result2);
    }
}
