using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.CustomSettings.Services;
public class CustomSettingsStereotypesProvider : IStereotypesProvider
{
    protected readonly IStringLocalizer S;

    public CustomSettingsStereotypesProvider(IStringLocalizer<CustomSettingsStereotypesProvider> stringLozalizer)
    {
        S = stringLozalizer;
    }

    public Task<IEnumerable<StereotypeDescription>> GetStereotypesAsync()
    {
        return Task.FromResult<IEnumerable<StereotypeDescription>>(
            [new StereotypeDescription { Stereotype = "CustomSettings", DisplayName = S["Custom Settings"] }]);
    }
}
