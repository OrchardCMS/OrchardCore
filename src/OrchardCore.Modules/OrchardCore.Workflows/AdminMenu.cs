using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Workflows
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
            if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder.Add(T["Workflows"], "5", workflow => workflow
                .AddClass("workflows").Id("workflows").Action("Index", "WorkflowType", new { area = "OrchardCore.Workflows" })
                    .Permission(Permissions.ManageWorkflows)
                    .LocalNav());

            return Task.CompletedTask;
        }
    }
}
