using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RMS.Application.Steps.Commands.DeleteStepDetail;
using RMS.Infrastructure.Data;
using Shouldly;
using NotFoundException = Ardalis.GuardClauses.NotFoundException;
using DomainStep = RMS.Domain.Entities.Models.Step;
using DomainStepDetail = RMS.Domain.Entities.Models.StepDetail;

namespace RMS.Application.UnitTests.Steps.Commands.DeleteStepDetail;

public class DeleteStepDetailCommandHandlerTests : IDisposable
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

    private async Task<DomainStepDetail> CreateStepDetailAsync()
    {
        var step = new DomainStep
        {
            Id = Guid.NewGuid(),
            Name = "Test Step",
            Order = 1
        };

        var detail = new DomainStepDetail
        {
            Id = Guid.NewGuid(),
            StepId = step.Id,
            Name = "To Delete",
            Order = 1
        };

        _dbContext.Steps.Add(step);
        _dbContext.StepDetails.Add(detail);
        await _dbContext.SaveChangesAsync();

        return detail;
    }

    [Test]
    public async Task Handle_ShouldDeleteStepDetail_WhenStepDetailExists()
    {
        // Arrange
        var detail = await CreateStepDetailAsync();
        var command = new DeleteStepDetailCommand(detail.Id);
        var handler = new DeleteStepDetailCommandHandler(_dbContext);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var count = await _dbContext.StepDetails.CountAsync();
        count.ShouldBe(0);
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenStepDetailDoesNotExist()
    {
        // Arrange
        var command = new DeleteStepDetailCommand(Guid.NewGuid());
        var handler = new DeleteStepDetailCommandHandler(_dbContext);

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ShouldRemoveFromContext()
    {
        // Arrange
        var detail = await CreateStepDetailAsync();
        var command = new DeleteStepDetailCommand(detail.Id);
        var handler = new DeleteStepDetailCommandHandler(_dbContext);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        (await _dbContext.StepDetails.CountAsync()).ShouldBe(0);
    }
}
