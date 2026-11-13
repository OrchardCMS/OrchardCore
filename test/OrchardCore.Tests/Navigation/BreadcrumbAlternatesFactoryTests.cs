using OrchardCore.Navigation;

namespace OrchardCore.Tests.Navigation;

public class BreadcrumbAlternatesFactoryTests
{
    [Fact]
    public void GetBreadcrumbAlternates_WithADisplayType_OrdersThemFromLeastToMostSpecific()
    {
        var alternates = BreadcrumbAlternatesFactory.GetBreadcrumbAlternates("ContentsEdit", "DetailAdmin");

        // The display engine reads the alternates backwards, so the most specific one has to be the last.
        Assert.Equal(
            [
                "Breadcrumb__ContentsEdit",
                "Breadcrumb_DetailAdmin",
                "Breadcrumb_DetailAdmin__ContentsEdit",
            ],
            alternates);
    }

    [Fact]
    public void GetBreadcrumbAlternates_WithoutADisplayType_OnlyNamesTheTrail()
    {
        var alternates = BreadcrumbAlternatesFactory.GetBreadcrumbAlternates("ContentsEdit", null);

        Assert.Equal(["Breadcrumb__ContentsEdit"], alternates);
    }

    [Fact]
    public void GetBreadcrumbItemAlternates_WithAnIdAndADisplayType_CombinesThem()
    {
        var alternates = BreadcrumbAlternatesFactory.GetBreadcrumbItemAlternates("ContentsEdit", "Dashboard", "DetailAdmin");

        Assert.Equal(
            [
                "BreadcrumbItem__ContentsEdit",
                "BreadcrumbItem__Dashboard",
                "BreadcrumbItem__ContentsEdit__Dashboard",
                "BreadcrumbItem_DetailAdmin",
                "BreadcrumbItem_DetailAdmin__ContentsEdit",
                "BreadcrumbItem_DetailAdmin__Dashboard",
                "BreadcrumbItem_DetailAdmin__ContentsEdit__Dashboard",
            ],
            alternates);
    }

    [Fact]
    public void GetBreadcrumbItemAlternates_WithoutAnId_LeavesOutTheNodeAlternates()
    {
        var alternates = BreadcrumbAlternatesFactory.GetBreadcrumbItemAlternates("ContentsEdit", null, "DetailAdmin");

        Assert.Equal(
            [
                "BreadcrumbItem__ContentsEdit",
                "BreadcrumbItem_DetailAdmin",
                "BreadcrumbItem_DetailAdmin__ContentsEdit",
            ],
            alternates);
    }

    [Fact]
    public void GetBreadcrumbAlternates_WithADottedName_EncodesTheDot()
    {
        var alternates = BreadcrumbAlternatesFactory.GetBreadcrumbAlternates("My.Trail", null);

        Assert.Equal(["Breadcrumb__My_Trail"], alternates);
    }

    [Fact]
    public void GetBreadcrumbItemAlternates_TitleWithoutDisplayType_UsesTrailNameAndTitleRole()
    {
        var alternates = BreadcrumbAlternatesFactory.GetBreadcrumbItemAlternates("ContentsEdit", "Title", null);

        Assert.Equal(
            [
                "BreadcrumbItem__ContentsEdit",
                "BreadcrumbItem__Title",
                "BreadcrumbItem__ContentsEdit__Title",
            ],
            alternates);
    }

    [Fact]
    public void GetBreadcrumbItemAlternates_TitleWithDisplayType_UsesTrailNameAndTitleRole()
    {
        var alternates = BreadcrumbAlternatesFactory.GetBreadcrumbItemAlternates("ContentsEdit", "Title", "DetailAdmin");

        Assert.Equal(
            [
                "BreadcrumbItem__ContentsEdit",
                "BreadcrumbItem__Title",
                "BreadcrumbItem__ContentsEdit__Title",
                "BreadcrumbItem_DetailAdmin",
                "BreadcrumbItem_DetailAdmin__ContentsEdit",
                "BreadcrumbItem_DetailAdmin__Title",
                "BreadcrumbItem_DetailAdmin__ContentsEdit__Title",
            ],
            alternates);
    }

    [Fact]
    public void GetBreadcrumbItemAlternates_TitleWithDottedName_EncodesTheTrailName()
    {
        var alternates = BreadcrumbAlternatesFactory.GetBreadcrumbItemAlternates("My.Trail", "Title", null);

        Assert.Equal(
            [
                "BreadcrumbItem__My_Trail",
                "BreadcrumbItem__Title",
                "BreadcrumbItem__My_Trail__Title",
            ],
            alternates);
    }
}
