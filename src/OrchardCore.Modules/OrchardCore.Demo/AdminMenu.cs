using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Demo
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(T["Demo"], "10", themes => themes
                    .AddClass("demo").Id("demo")
                    .Add(T["This Menu Item 1.1"], "0", installed => installed
                        .Add(T["This is Menu Item 1.1"])
                        .Add(T["This is Menu Item 1.2"])
                        .Add(T["This is Menu Item 1.2"])
                    )
                    .Add(T["This Menu Item 2.1"], "0", installed => installed
                        .Add(T["This is Menu Item 2.1"])
                        .Add(T["This is Menu Item 2.2"])
                        .Add(T["This is Menu Item 3.2"])
                    )
                    .Add(T["This Menu Item 3.1"], "0", installed => installed
                        .Add(T["This is Menu Item 3.1"])
                        .Add(T["This is Menu Item 3.2"])
                    )
                );

            return Task.CompletedTask;

        }
    }
}
