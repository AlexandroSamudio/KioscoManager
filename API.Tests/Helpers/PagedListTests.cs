using API.Helpers;
using FluentAssertions;

namespace API.Tests.Helpers;

public class PagedListTests
{
    [Fact]
    public void Constructor_SetsPaginationFields_Correctly_PageSize10_Count100()
    {
        var items = new List<int> { 1,2,3,4,5,6,7,8,9,10 };

        var paged = new PagedList<int>(items, count: 100, pageNumber: 1, pageSize: 10);

        paged.CurrentPage.Should().Be(1);
        paged.PageSize.Should().Be(10);
        paged.TotalCount.Should().Be(100);
        paged.TotalPages.Should().Be(10);
        paged.Should().ContainInOrder(items);
    }

    [Fact]
    public void Constructor_ComputesTotalPages_WithRemainder()
    {
        var items = new List<int> { 1, 2, 3 };

        var paged = new PagedList<int>(items, count: 10, pageNumber: 2, pageSize: 3);
        
        paged.TotalPages.Should().Be(4);
        paged.CurrentPage.Should().Be(2);
        paged.Count.Should().Be(3);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, -5)]
    public void Constructor_Throws_OnNonPositivePageNumberOrPageSize(int pageNumber, int pageSize)
    {
        var act = () => new PagedList<int>(new List<int>(), count: 0, pageNumber: pageNumber, pageSize: pageSize);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
