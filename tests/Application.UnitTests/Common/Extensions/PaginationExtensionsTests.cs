using NUnit.Framework;
using RMS.Application.Common.Models;
using Shouldly;

namespace RMS.Application.UnitTests.Common.Extensions;

public class PaginationExtensionsTests
{
    [Test]
    public void TotalPages_ShouldBeCalculatedCorrectly_ForRemainder()
    {
        var result = new PaginatedResult<string>(
            new List<string> { "item1", "item2" }, 7, 1, 3);

        result.TotalPages.ShouldBe(3);
    }

    [Test]
    public void TotalPages_ShouldBeCalculatedCorrectly_ForExactDivision()
    {
        var result = new PaginatedResult<string>(
            new List<string> { "item1" }, 10, 1, 5);

        result.TotalPages.ShouldBe(2);
    }

    [Test]
    public void TotalPages_ShouldBeZero_WhenTotalCountIsZero()
    {
        var result = new PaginatedResult<string>(
            new List<string>(), 0, 1, 10);

        result.TotalPages.ShouldBe(0);
    }

    [Test]
    public void TotalPages_ShouldBeOne_WhenFewerItemsThanPageSize()
    {
        var result = new PaginatedResult<string>(
            new List<string> { "item1" }, 1, 1, 10);

        result.TotalPages.ShouldBe(1);
    }

    [Test]
    public void TotalPages_ShouldRoundUp_ForPartialPage()
    {
        var result = new PaginatedResult<string>(
            new List<string> { "item1" }, 11, 1, 10);

        result.TotalPages.ShouldBe(2);
    }

    [Test]
    public void ShouldPreserveItems()
    {
        var items = new List<string> { "a", "b", "c" };
        var result = new PaginatedResult<string>(items, 3, 1, 10);

        result.Items.ShouldBeEquivalentTo(items);
    }

    [Test]
    public void ShouldPreserveMetadata()
    {
        var result = new PaginatedResult<string>(
            new List<string> { "a", "b" }, 50, 3, 10);

        result.TotalCount.ShouldBe(50);
        result.PageNumber.ShouldBe(3);
        result.PageSize.ShouldBe(10);
    }
}
