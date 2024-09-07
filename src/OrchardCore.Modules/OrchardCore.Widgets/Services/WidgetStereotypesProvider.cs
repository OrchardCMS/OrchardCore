using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Widgets.Services;

public class WidgetStereotypesProvider : IStereotypesProvider
{
    protected readonly IStringLocalizer S;

    public WidgetStereotypesProvider(IStringLocalizer<WidgetStereotypesProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task<IEnumerable<StereotypeDescription>> GetStereotypesAsync()
        => Task.FromResult<IEnumerable<StereotypeDescription>>(
            [new StereotypeDescription { Stereotype = "Widget", DisplayName = S["Widget"] }]);
}
