using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Menu.Services;

public class MenuItemStereotypesProvider : IStereotypesProvider
{
    protected readonly IStringLocalizer S;

    public MenuItemStereotypesProvider(IStringLocalizer<MenuItemStereotypesProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task<IEnumerable<StereotypeDescription>> GetStereotypesAsync()
        => Task.FromResult<IEnumerable<StereotypeDescription>>(
          [new StereotypeDescription { Stereotype = "MenuItem", DisplayName = S["Menu Item"] }]);
}
