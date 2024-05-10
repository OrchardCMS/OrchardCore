using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Users.Services;
public class CustomUserSettingsStereotypesProvider : IStereotypesProvider
{
    protected readonly IStringLocalizer S;

    public CustomUserSettingsStereotypesProvider(IStringLocalizer stringLozalizer)
    {
        S = stringLozalizer;
    }

    public Task<IEnumerable<StereotypeDescription>> GetStereotypesAsync()
    {
        return Task.FromResult<IEnumerable<StereotypeDescription>>(
            [new StereotypeDescription { Stereotype = "CustomUserSettings", DisplayName = S["Custom User Settings"] }]);
    }
}
