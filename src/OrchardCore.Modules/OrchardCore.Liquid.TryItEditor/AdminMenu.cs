using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Liquid.TryItEditor
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder
                    .Add(S["Liquid TryIt Editor"], TryItEditorConstants.NavigationConstants.AdminMenuTryItEditorPosition, liquidtryiteditor => liquidtryiteditor
                        .AddClass("liquidtryiteditor").Id("liquidtryiteditor")
                        .Action("Index", "Admin", new { area = TryItEditorConstants.Features.TryItEditor })
                            .Permission(Permissions.UseTryItEditor)
                            .LocalNav());
            }
            
            return Task.CompletedTask;
        }
    }
}
