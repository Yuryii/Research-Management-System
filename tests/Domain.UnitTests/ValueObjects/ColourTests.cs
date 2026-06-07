using NUnit.Framework;
using RMS.Domain.Exceptions;
using RMS.Domain.ValueObjects;
using Shouldly;

namespace RMS.Domain.UnitTests.ValueObjects;

public class ColourTests
{
    [Test]
    public void ShouldReturnCorrectColourCode()
    {
        // Arrange
        var code = "#E05C4D";

        // Act
        var colour = Colour.From(code);

        // Assert
        colour.Code.ShouldBe(code);
    }

    [Test]
    public void ToStringReturnsCode()
    {
        // Arrange
        var colour = Colour.Red;

        // Act & Assert
        colour.ToString().ShouldBe(colour.Code);
    }

    [Test]
    public void ShouldPerformImplicitConversionToColourCodeString()
    {
        // Arrange
        var colour = Colour.Red;

        // Act
        string code = colour;

        // Assert
        code.ShouldBe("#E05C4D");
    }

    [Test]
    public void ShouldPerformExplicitConversionGivenSupportedColourCode()
    {
        // Arrange & Act
        var colour = (Colour)"#E05C4D";

        // Assert
        colour.ShouldBe(Colour.Red);
    }

    [Test]
    public void ShouldThrowUnsupportedColourExceptionGivenNotSupportedColourCode()
    {
        // Act & Assert
        Should.Throw<UnsupportedColourException>(() => Colour.From("##FF33CC"));
    }

    [Test]
    public void ShouldBeComparableWithOperators()
    {
        // Arrange
        var color1 = new Colour("#E05C4D");
        var color2 = new Colour("#E05C4D");
        var color3 = new Colour("#AAAAAA");

        // Act & Assert
        (color1 == color2).ShouldBe(true);
        (color1 == color3).ShouldBe(false);
    }

    [Test]
    public void ShouldReturnDefaultBlackForNullOrEmptyCode()
    {
        // Arrange & Act
        var colourNull = new Colour(null!);
        var colourEmpty = new Colour(string.Empty);

        // Assert
        colourNull.Code.ShouldBe("#000000");
        colourEmpty.Code.ShouldBe("#000000");
    }

    [Test]
    public void ShouldSupportAllStaticColours()
    {
        // Act & Assert
        Colour.Red.Code.ShouldBe("#E05C4D");
        Colour.Orange.Code.ShouldBe("#D98B2B");
        Colour.Green.Code.ShouldBe("#4CAF50");
        Colour.Teal.Code.ShouldBe("#26A69A");
        Colour.Blue.Code.ShouldBe("#5C6BC0");
        Colour.Purple.Code.ShouldBe("#AB47BC");
        Colour.Grey.Code.ShouldBe("#78909C");
    }
}
