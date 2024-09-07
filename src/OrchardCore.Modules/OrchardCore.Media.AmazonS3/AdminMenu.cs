using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Media.AmazonS3;

public sealed class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder.Add(S["Configuration"], configuration => configuration
            .Add(S["Media"], S["Media"].PrefixPosition(), media => media
                .Add(S["Amazon S3 Options"], S["Amazon S3 Options"].PrefixPosition(), options => options
                    .Action("Options", "Admin", "OrchardCore.Media.AmazonS3")
                    .Permission(Permissions.ViewAmazonS3MediaOptions)
                    .LocalNav()
                )
            )
        );

        return ValueTask.CompletedTask;
    }
}
