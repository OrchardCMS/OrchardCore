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
