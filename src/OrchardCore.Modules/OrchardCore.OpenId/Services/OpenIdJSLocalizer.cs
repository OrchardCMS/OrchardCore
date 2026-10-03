using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;

namespace OrchardCore.OpenId.Services;

public sealed class OpenIdJSLocalizer(IStringLocalizer<OpenIdJSLocalizer> S) : IJSLocalizer
{
    public IDictionary<string, string> GetLocalizations(string group)
    {
        if (string.Equals(group, "openid-parameters-editor", StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>
            {
                { "ParameterNameColumn", S["Parameter Name"].Value },
                { "EnterAParameter", S["Enter a parameter"].Value },
                { "ValueColumn", S["Value"].Value },
                { "EnterAValue", S["Enter a value"].Value },
                { "AddAParameter", S["Add a parameter"].Value },
                { "EditData", S["Edit Data"].Value },
                { "Ok", S["OK"].Value },
                { "Cancel", S["Cancel"].Value },
                { "RemoveParameterFromList", S["Remove parameter from list"].Value },
                { "Parameters", S["Parameters"].Value },
                {
                    "ParametersJsonHint",
                    S["A JSON representation of the allowed values, e.g. {0}", "[ { name: 'First parameter', value: 'parameter1' }, { name: 'Second parameter', value: 'parameter2' } ]"].Value
                },
            };
        }

        return null;
    }
}
