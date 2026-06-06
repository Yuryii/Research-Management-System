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
        var code = "#E05C4D";

        var colour = Colour.From(code);

        colour.Code.ShouldBe(code);
    }

    [Test]
    public void ToStringReturnsCode()
    {
        var colour = Colour.Red;

        colour.ToString().ShouldBe(colour.Code);
    }

    [Test]
    public void ShouldPerformImplicitConversionToColourCodeString()
    {
        string code = Colour.Red;

        code.ShouldBe("#E05C4D");
    }

    [Test]
    public void ShouldPerformExplicitConversionGivenSupportedColourCode()
    {
        var colour = (Colour)"#E05C4D";

        colour.ShouldBe(Colour.Red);
    }

    [Test]
    public void ShouldThrowUnsupportedColourExceptionGivenNotSupportedColourCode()
    {
        Should.Throw<UnsupportedColourException>(() => Colour.From("##FF33CC"));
    }

    [Test]
    public void ShouldBeComparableWithOperators()
    {
        var color1 = new Colour("#E05C4D");
        var color2 = new Colour("#E05C4D");
        var color3 = new Colour("#AAAAAA");
        (color1 == color2).ShouldBe(true);
        (color1 == color3).ShouldBe(false);
    }

    [Test]
    public void ShouldReturnDefaultBlackForNullOrEmptyCode()
    {
        var colourNull = new Colour(null!);
        var colourEmpty = new Colour(string.Empty);

        colourNull.Code.ShouldBe("#000000");
        colourEmpty.Code.ShouldBe("#000000");
    }

    [Test]
    public void ShouldSupportAllStaticColours()
    {
        Colour.Red.Code.ShouldBe("#E05C4D");
        Colour.Orange.Code.ShouldBe("#D98B2B");
        Colour.Green.Code.ShouldBe("#4CAF50");
        Colour.Teal.Code.ShouldBe("#26A69A");
        Colour.Blue.Code.ShouldBe("#5C6BC0");
        Colour.Purple.Code.ShouldBe("#AB47BC");
        Colour.Grey.Code.ShouldBe("#78909C");
    }
}
