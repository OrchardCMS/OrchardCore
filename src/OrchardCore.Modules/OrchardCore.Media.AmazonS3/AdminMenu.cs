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
        if (NavigationHelper.UseLegacyFormat())
        {
            builder.Add(S["Configuration"], configuration => configuration
                .Add(S["Media"], S["Media"].PrefixPosition(), media => media
                    .Add(S["Amazon S3 options"], S["Amazon S3 options"].PrefixPosition(), options => options
                        .Action("Options", "Admin", "OrchardCore.Media.AmazonS3")
                        .Permission(Permissions.ViewAmazonS3MediaOptions)
                        .LocalNav()
                    )
                )
            );

            return ValueTask.CompletedTask;
        }

        builder.Add(S["Tools"], tools => tools
            .Add(S["Media"], S["Media"].PrefixPosition(), media => media
                .Add(S["Amazon S3 options"], S["Amazon S3 options"].PrefixPosition(), options => options
                    .Action("Options", "Admin", "OrchardCore.Media.AmazonS3")
                    .Permission(Permissions.ViewAmazonS3MediaOptions)
                    .LocalNav()
                )
            )
        );

        return ValueTask.CompletedTask;
    }
}
