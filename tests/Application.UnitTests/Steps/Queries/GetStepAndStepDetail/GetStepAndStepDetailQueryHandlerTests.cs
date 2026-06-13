using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RMS.Application.Common.Interfaces;
using RMS.Application.Steps.Dtos;
using RMS.Application.Steps.Queries.GetStepAndStepDetail;
using RMS.Infrastructure.Data;
using Shouldly;
using DomainStep = RMS.Domain.Entities.Models.Step;
using DomainStepDetail = RMS.Domain.Entities.Models.StepDetail;

namespace RMS.Application.UnitTests.Steps.Queries.GetStepAndStepDetail;

public class GetStepAndStepDetailQueryHandlerTests : IDisposable
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<IMapper> _mapperMock = null!;

    private static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static IConfigurationProvider CreateConfigurationProvider()
    {
        var expr = new MapperConfigurationExpression();
        expr.AddMaps(typeof(StepDto).Assembly);
        var config = new MapperConfiguration(expr, new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory());
        config.AssertConfigurationIsValid();
        return config;
    }

    [SetUp]
    public void SetUp()
    {
        _dbContext = CreateInMemoryContext();
        _mapperMock = new Mock<IMapper>();

        var configProvider = CreateConfigurationProvider();
        _mapperMock.Setup(m => m.ConfigurationProvider).Returns(configProvider);
        _mapperMock.Setup(m => m.Map<IList<StepDto>>(It.IsAny<List<DomainStep>>()))
            .Returns((List<DomainStep>? steps) => steps == null
                ? new List<StepDto>()
                : steps.Select(s => new StepDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    ShortName = s.ShortName,
                    Order = s.Order,
                    StepDetails = s.StepDetails.OrderBy(sd => sd.Order).Select(sd => new StepDetailDto
                    {
                        Id = sd.Id,
                        StepId = sd.StepId,
                        Name = sd.Name,
                        Order = sd.Order
                    }).ToList()
                }).ToList());
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

    private DomainStep CreateStep(Guid id, string name, string shortName, int order)
    {
        var step = new DomainStep
        {
            Id = id,
            Name = name,
            ShortName = shortName,
            Order = order
        };
        _dbContext.Steps.Add(step);
        return step;
    }

    private DomainStepDetail CreateStepDetail(Guid id, Guid stepId, string name, int order)
    {
        var stepDetail = new DomainStepDetail
        {
            Id = id,
            StepId = stepId,
            Name = name,
            Order = order
        };
        _dbContext.StepDetails.Add(stepDetail);
        return stepDetail;
    }

    [Test]
    public async Task Handle_ShouldReturnAllSteps_WhenStepsExist()
    {
        // Arrange
        var step1Id = Guid.NewGuid();
        var step2Id = Guid.NewGuid();
        CreateStep(step1Id, "Step One", "S1", 1);
        CreateStep(step2Id, "Step Two", "S2", 2);
        CreateStepDetail(Guid.NewGuid(), step1Id, "Detail 1", 1);
        CreateStepDetail(Guid.NewGuid(), step2Id, "Detail 2", 1);
        _dbContext.SaveChanges();

        var handler = new GetStepAndStepDetailQueryHandler(_dbContext, _mapperMock.Object);
        var query = new GetStepAndStepDetailQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
    }

    [Test]
    public async Task Handle_ShouldReturnEmptyList_WhenNoStepsExist()
    {
        // Arrange
        var handler = new GetStepAndStepDetailQueryHandler(_dbContext, _mapperMock.Object);
        var query = new GetStepAndStepDetailQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task Handle_ShouldReturnStepsOrderedByOrder()
    {
        // Arrange
        var step1Id = Guid.NewGuid();
        var step2Id = Guid.NewGuid();
        var step3Id = Guid.NewGuid();
        CreateStep(step1Id, "Third", "T3", 3);
        CreateStep(step2Id, "First", "F1", 1);
        CreateStep(step3Id, "Second", "S2", 2);
        CreateStepDetail(Guid.NewGuid(), step1Id, "D1", 1);
        CreateStepDetail(Guid.NewGuid(), step2Id, "D2", 1);
        CreateStepDetail(Guid.NewGuid(), step3Id, "D3", 1);
        _dbContext.SaveChanges();

        var handler = new GetStepAndStepDetailQueryHandler(_dbContext, _mapperMock.Object);
        var query = new GetStepAndStepDetailQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result[0].Order.ShouldBe(1);
        result[1].Order.ShouldBe(2);
        result[2].Order.ShouldBe(3);
    }

    [Test]
    public async Task Handle_ShouldIncludeStepDetails()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        CreateStep(stepId, "Step With Details", "SWD", 1);
        CreateStepDetail(Guid.NewGuid(), stepId, "Detail 1", 1);
        CreateStepDetail(Guid.NewGuid(), stepId, "Detail 2", 2);
        _dbContext.SaveChanges();

        var handler = new GetStepAndStepDetailQueryHandler(_dbContext, _mapperMock.Object);
        var query = new GetStepAndStepDetailQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].StepDetails.Count.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Test]
    public async Task Handle_ShouldReturnStepsWithNullShortName()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        CreateStep(stepId, "Step Without ShortName", "", 1);
        _dbContext.SaveChanges();

        var handler = new GetStepAndStepDetailQueryHandler(_dbContext, _mapperMock.Object);
        var query = new GetStepAndStepDetailQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Step Without ShortName");
    }
}
