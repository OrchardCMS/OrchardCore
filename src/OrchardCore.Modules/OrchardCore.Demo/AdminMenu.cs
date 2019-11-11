using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Demo
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(T["Demo"], "10", demo => demo
                    .AddClass("demo").Id("demo")
                    .Add(T["This Menu Item 1"], "0", item => item
                        .Add(T["This is Menu Item 1.1"], subItem => subItem
                            .Action("Index", "Admin", new { area = "OrchardCore.Demo" }))
                        .Add(T["This is Menu Item 1.2"], subItem => subItem
                            .Action("Index", "Admin", new { area = "OrchardCore.Demo" }))
                        .Add(T["This is Menu Item 1.2"], subItem => subItem
                            .Action("Index", "Admin", new { area = "OrchardCore.Demo" }))
                    )
                    .Add(T["This Menu Item 2"], "0", item => item
                        .Add(T["This is Menu Item 2.1"], subItem => subItem
                            .Action("Index", "Admin", new { area = "OrchardCore.Demo" }))
                        .Add(T["This is Menu Item 2.2"], subItem => subItem
                            .Action("Index", "Admin", new { area = "OrchardCore.Demo" }))
                        .Add(T["This is Menu Item 3.2"], subItem => subItem
                            .Action("Index", "Admin", new { area = "OrchardCore.Demo" }))
                    )
                    .Add(T["This Menu Item 3"], "0", item => item
                        .Add(T["This is Menu Item 3.1"], subItem => subItem
                            .Action("Index", "Admin", new { area = "OrchardCore.Demo" }))
                        .Add(T["This is Menu Item 3.2"], subItem => subItem
                            .Action("Index", "Admin", new { area = "OrchardCore.Demo" }))
                    )
                );

            return Task.CompletedTask;
        }
    }
}
