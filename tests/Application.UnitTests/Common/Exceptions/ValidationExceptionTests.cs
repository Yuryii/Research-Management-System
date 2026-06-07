using FluentValidation.Results;
using NUnit.Framework;
using RMS.Application.Common.Exceptions;
using Shouldly;

namespace RMS.Application.UnitTests.Common.Exceptions;

public class ValidationExceptionTests
{
    [Test]
    public void DefaultConstructorCreatesAnEmptyErrorDictionary()
    {
        // Arrange & Act
        var actual = new ValidationException().Errors;

        // Assert
        actual.Keys.ShouldBeEmpty();
    }

    [Test]
    public void SingleValidationFailureCreatesASingleElementErrorDictionary()
    {
        // Arrange
        var failures = new List<ValidationFailure>
            {
                new ValidationFailure("Age", "must be over 18"),
            };

        // Act
        var actual = new ValidationException(failures).Errors;

        // Assert
        actual.Keys.ShouldBe(new string[] { "Age" });
        actual["Age"].ShouldBe(new string[] { "must be over 18" });
    }

    [Test]
    public void MulitpleValidationFailureForMultiplePropertiesCreatesAMultipleElementErrorDictionaryEachWithMultipleValues()
    {
        // Arrange
        var failures = new List<ValidationFailure>
            {
                new ValidationFailure("Age", "must be 18 or older"),
                new ValidationFailure("Age", "must be 25 or younger"),
                new ValidationFailure("Password", "must contain at least 8 characters"),
                new ValidationFailure("Password", "must contain a digit"),
                new ValidationFailure("Password", "must contain upper case letter"),
                new ValidationFailure("Password", "must contain lower case letter"),
            };

        // Act
        var actual = new ValidationException(failures).Errors;

        // Assert
        actual.Keys.ShouldBe(new string[] { "Password", "Age" }, ignoreOrder: true);

        actual["Age"].ShouldBe(new string[]
        {
                "must be 25 or younger",
                "must be 18 or older",
        }, ignoreOrder: true);

        actual["Password"].ShouldBe(new string[]
        {
                "must contain lower case letter",
                "must contain upper case letter",
                "must contain at least 8 characters",
                "must contain a digit",
        }, ignoreOrder: true);
    }
}
