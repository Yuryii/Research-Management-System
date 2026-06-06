using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NUnit.Framework;
using RMS.Application.Application.Commands.CreateApplication;
using RMS.Application.Common.Behaviours;
using Shouldly;
using AppValidationException = RMS.Application.Common.Exceptions.ValidationException;

namespace RMS.Application.UnitTests.Common.Behaviours;

public class ValidationBehaviourTests
{
    [Test]
    public async Task Handle_NoValidators_CallsNextDelegate()
    {
        // Arrange
        var validators = new List<IValidator<CreateApplicationCommand>>();
        var sut = new ValidationBehaviour<CreateApplicationCommand, Guid>(validators);

        var request = new CreateApplicationCommand { Title = "Test", Description = "Description" };
        var expectedResponse = Guid.NewGuid();
        RequestHandlerDelegate<Guid> next = (ct) => Task.FromResult(expectedResponse);

        // Act
        var result = await sut.Handle(request, next, CancellationToken.None);

        // Assert
        result.ShouldBe(expectedResponse);
    }

    [Test]
    public async Task Handle_ValidatorPasses_CallsNextDelegate()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<CreateApplicationCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CreateApplicationCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var sut = new ValidationBehaviour<CreateApplicationCommand, Guid>(
            new List<IValidator<CreateApplicationCommand>> { validatorMock.Object });

        var request = new CreateApplicationCommand { Title = "Test", Description = "Description" };
        var expectedResponse = Guid.NewGuid();
        RequestHandlerDelegate<Guid> next = (ct) => Task.FromResult(expectedResponse);

        // Act
        var result = await sut.Handle(request, next, CancellationToken.None);

        // Assert
        result.ShouldBe(expectedResponse);
    }

    [Test]
    public async Task Handle_ValidatorFails_ThrowsValidationException()
    {
        // Arrange
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Title", "Title is required.")
        };

        var validatorMock = new Mock<IValidator<CreateApplicationCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CreateApplicationCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        var sut = new ValidationBehaviour<CreateApplicationCommand, Guid>(
            new List<IValidator<CreateApplicationCommand>> { validatorMock.Object });

        var request = new CreateApplicationCommand { Title = "", Description = "Description" };
        RequestHandlerDelegate<Guid> next = (ct) => Task.FromResult(Guid.NewGuid());

        // Act & Assert
        var exception = await Should.ThrowAsync<AppValidationException>(() => sut.Handle(request, next, CancellationToken.None));
        exception.Errors.ShouldContainKey("Title");
        exception.Errors["Title"].ShouldContain("Title is required.");
    }

    [Test]
    public async Task Handle_MultipleValidatorFailures_CollectsAllErrors()
    {
        // Arrange
        var failures1 = new List<ValidationFailure>
        {
            new ValidationFailure("Title", "Title is required.")
        };

        var failures2 = new List<ValidationFailure>
        {
            new ValidationFailure("Description", "Description is required.")
        };

        var validatorMock1 = new Mock<IValidator<CreateApplicationCommand>>();
        validatorMock1.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CreateApplicationCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures1));

        var validatorMock2 = new Mock<IValidator<CreateApplicationCommand>>();
        validatorMock2.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CreateApplicationCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures2));

        var sut = new ValidationBehaviour<CreateApplicationCommand, Guid>(
            new List<IValidator<CreateApplicationCommand>> { validatorMock1.Object, validatorMock2.Object });

        var request = new CreateApplicationCommand { Title = "", Description = "" };
        RequestHandlerDelegate<Guid> next = (ct) => Task.FromResult(Guid.NewGuid());

        // Act & Assert
        var exception = await Should.ThrowAsync<AppValidationException>(() => sut.Handle(request, next, CancellationToken.None));
        exception.Errors.Keys.ShouldContain("Title");
        exception.Errors.Keys.ShouldContain("Description");
    }

    [Test]
    public async Task Handle_MultipleFailuresForSameProperty_GroupsErrorsByProperty()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("Title", "Title is required."),
            new ValidationFailure("Title", "Title must not exceed 200 characters.")
        };

        var validatorMock = new Mock<IValidator<CreateApplicationCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CreateApplicationCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        var sut = new ValidationBehaviour<CreateApplicationCommand, Guid>(
            new List<IValidator<CreateApplicationCommand>> { validatorMock.Object });

        var request = new CreateApplicationCommand { Title = "", Description = "" };
        RequestHandlerDelegate<Guid> next = (ct) => Task.FromResult(Guid.NewGuid());

        // Act & Assert
        var exception = await Should.ThrowAsync<AppValidationException>(() => sut.Handle(request, next, CancellationToken.None));
        exception.Errors["Title"].Length.ShouldBe(2);
    }

    [Test]
    public async Task Handle_NoFailures_DoesNotThrow()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<CreateApplicationCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CreateApplicationCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var sut = new ValidationBehaviour<CreateApplicationCommand, Guid>(
            new List<IValidator<CreateApplicationCommand>> { validatorMock.Object });

        var request = new CreateApplicationCommand { Title = "Valid Title", Description = "Valid Description" };
        var expectedResponse = Guid.NewGuid();
        RequestHandlerDelegate<Guid> next = (ct) => Task.FromResult(expectedResponse);

        // Act
        var result = await sut.Handle(request, next, CancellationToken.None);

        // Assert
        result.ShouldBe(expectedResponse);
    }

    [Test]
    public async Task Handle_EmptyValidatorsList_CallsNextDelegate()
    {
        // Arrange
        var sut = new ValidationBehaviour<CreateApplicationCommand, Guid>(
            new List<IValidator<CreateApplicationCommand>>());

        var request = new CreateApplicationCommand { Title = "Test", Description = "Description" };
        var expectedResponse = Guid.NewGuid();
        RequestHandlerDelegate<Guid> next = (ct) => Task.FromResult(expectedResponse);

        // Act
        var result = await sut.Handle(request, next, CancellationToken.None);

        // Assert
        result.ShouldBe(expectedResponse);
    }

}
