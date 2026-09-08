using OrchardCore.Navigation;

namespace OrchardCore.Tests.Navigation;

public class PagerOptionsTests
{
    [Fact]
    public void GetPageSize_ReturnsDefault_WhenSelectionIsDisabled()
    {
        var options = new PagerOptions
        {
            PageSize = 10,
            AllowPageSizeSelection = false,
            PageSizeOptions = [10, 25, 50, 100],
        };

        // Even a value that is in the options list is ignored while selection is disabled.
        Assert.Equal(10, options.GetPageSize(25));
    }

    [Fact]
    public void GetPageSize_HonorsSelectedValue_WhenAllowedAndConfigured()
    {
        var options = new PagerOptions
        {
            PageSize = 10,
            AllowPageSizeSelection = true,
            PageSizeOptions = [10, 25, 50, 100],
        };

        Assert.Equal(25, options.GetPageSize(25));
    }

    [Theory]
    [InlineData(15)]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(1000)]
    public void GetPageSize_ReturnsDefault_WhenSelectedValueIsNotAllowed(int selected)
    {
        var options = new PagerOptions
        {
            PageSize = 10,
            AllowPageSizeSelection = true,
            PageSizeOptions = [10, 25, 50, 100],
        };

        Assert.Equal(10, options.GetPageSize(selected));
    }

    [Fact]
    public void GetPageSize_ReturnsDefault_WhenNoValueIsSelected()
    {
        var options = new PagerOptions
        {
            PageSize = 10,
            AllowPageSizeSelection = true,
            PageSizeOptions = [10, 25, 50, 100],
        };

        Assert.Equal(10, options.GetPageSize(null));
    }

    [Fact]
    public void GetPageSize_ReturnsDefault_WhenOptionsAreEmpty()
    {
        var options = new PagerOptions
        {
            PageSize = 10,
            AllowPageSizeSelection = true,
            PageSizeOptions = [],
        };

        Assert.Equal(10, options.GetPageSize(25));
    }

    [Fact]
    public void GetPageSize_ClampsSelectedValueToMaxPageSize()
    {
        var options = new PagerOptions
        {
            PageSize = 10,
            MaxPageSize = 50,
            AllowPageSizeSelection = true,
            PageSizeOptions = [10, 25, 100],
        };

        // 100 is an allowed option but exceeds MaxPageSize, so it is clamped.
        Assert.Equal(50, options.GetPageSize(100));
    }

    [Fact]
    public void PageSizeOptions_DefaultsToCommonValues()
    {
        var options = new PagerOptions();

        Assert.Equal([10, 25, 50, 100], options.PageSizeOptions);
    }

    [Fact]
    public void GetPageSize_WithExplicitDefault_HonorsAllowedSelectedValue()
    {
        var options = new PagerOptions
        {
            PageSize = 10,
            AllowPageSizeSelection = true,
            PageSizeOptions = [10, 25, 50, 100],
        };

        // The explicit default (a list's own page size) is used only as a fallback.
        Assert.Equal(50, options.GetPageSize(50, 20));
    }

    [Fact]
    public void GetPageSize_WithExplicitDefault_FallsBackWhenNotAllowed()
    {
        var options = new PagerOptions
        {
            PageSize = 10,
            AllowPageSizeSelection = true,
            PageSizeOptions = [10, 25, 50, 100],
        };

        Assert.Equal(20, options.GetPageSize(33, 20));
    }

    [Fact]
    public void GetPageSize_WithExplicitDefault_FallsBackWhenDisabled()
    {
        var options = new PagerOptions
        {
            PageSize = 10,
            AllowPageSizeSelection = false,
            PageSizeOptions = [10, 25, 50, 100],
        };

        Assert.Equal(20, options.GetPageSize(50, 20));
    }

    [Fact]
    public void PagerConstructor_UsesResolvedPageSizeFromOptions()
    {
        var options = new PagerOptions
        {
            PageSize = 10,
            AllowPageSizeSelection = true,
            PageSizeOptions = [10, 25, 50, 100],
        };

        var pager = new Pager(new PagerParameters { Page = 2, PageSize = 25 }, options);

        Assert.Equal(2, pager.Page);
        Assert.Equal(25, pager.PageSize);
    }

    [Fact]
    public void PagerConstructor_FallsBackToDefault_WhenRequestedPageSizeIsNotAllowed()
    {
        var options = new PagerOptions
        {
            PageSize = 10,
            AllowPageSizeSelection = true,
            PageSizeOptions = [10, 25, 50, 100],
        };

        var pager = new Pager(new PagerParameters { PageSize = 999 }, options);

        Assert.Equal(1, pager.Page);
        Assert.Equal(10, pager.PageSize);
    }
}
