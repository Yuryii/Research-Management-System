using System;
using NUnit.Framework;
using RMS.Domain.Entities;
using Shouldly;

namespace RMS.Domain.UnitTests.Entities;

public class ApplicationReturnTests
{
    [Test]
    public void ShouldInitializeApplicationReturnFilesAsEmptyList()
    {
        // Arrange & Act
        var returnEntity = new ApplicationReturn
        {
            Title = "Title",
            Description = "Description"
        };

        // Assert
        returnEntity.ApplicationReturnFiles.ShouldBeEmpty();
    }

    [Test]
    public void ShouldSetAndGetAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var returnEntity = new ApplicationReturn
        {
            Id = id,
            Title = "Missing Documents",
            Description = "Please submit your ID card scan",
            RecipientId = "user-123"
        };

        // Assert
        returnEntity.Id.ShouldBe(id);
        returnEntity.Title.ShouldBe("Missing Documents");
        returnEntity.Description.ShouldBe("Please submit your ID card scan");
        returnEntity.RecipientId.ShouldBe("user-123");
    }

    [Test]
    public void ShouldSetRecipientId()
    {
        // Arrange
        var userId = "teacher-001";

        // Act
        var returnEntity = new ApplicationReturn
        {
            Title = "Title",
            Description = "Description",
            RecipientId = userId
        };

        // Assert
        returnEntity.RecipientId.ShouldBe(userId);
    }

    [Test]
    public void ShouldSetRequiredTitle()
    {
        // Arrange & Act
        var returnEntity = new ApplicationReturn
        {
            Title = "Incomplete Form",
            Description = "Please fill all required fields"
        };

        // Assert
        returnEntity.Title.ShouldBe("Incomplete Form");
    }

    [Test]
    public void ShouldSetRequiredDescription()
    {
        // Arrange & Act
        var returnEntity = new ApplicationReturn
        {
            Title = "Missing Info",
            Description = "Please provide your research proposal"
        };

        // Assert
        returnEntity.Description.ShouldBe("Please provide your research proposal");
    }
}
