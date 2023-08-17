using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Media.AmazonS3
{
    public class AdminMenu : INavigationProvider
    {
        protected readonly IStringLocalizer S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder.Add(S["Configuration"], configuration => configuration
                .Add(S["Media"], S["Media"].PrefixPosition(), media => media
                    .Add(S["Amazon S3 Options"], S["Amazon S3 Options"].PrefixPosition(), options => options
                        .Action("Options", "Admin", new { area = "OrchardCore.Media.AmazonS3" })
                        .Permission(Permissions.ViewAmazonS3MediaOptions)
                        .LocalNav())
            ));

            return Task.CompletedTask;
        }
    }
}
