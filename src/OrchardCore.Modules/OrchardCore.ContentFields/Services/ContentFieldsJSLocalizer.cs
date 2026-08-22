using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;

namespace OrchardCore.ContentFields.Services;

public sealed class ContentFieldsJSLocalizer(IStringLocalizer<ContentFieldsJSLocalizer> S) : IJSLocalizer
{
    public IDictionary<string, string> GetLocalizations(string group)
    {
        if (string.Equals(group, "options-table-editor", StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>
            {
                { "OptionLabelColumn", S["Option Label"].Value },
                { "EnterAName", S["Enter a name"].Value },
                { "ValueColumn", S["Value"].Value },
                { "EnterAValue", S["Enter a value"].Value },
                { "DefaultColumn", S["Default?"].Value },
                { "AddAnOption", S["Add an option"].Value },
                { "EditData", S["Edit Data"].Value },
                { "Ok", S["OK"].Value },
                { "Cancel", S["Cancel"].Value },
                { "RemoveElementFromList", S["Remove element from list"].Value },
                { "Options", S["Options"].Value },
                {
                    "OptionsJsonHint",
                    S["A JSON representation of the allowed values, e.g. {0}", "[ { name: 'First option', value: 'option1' }, { name: 'Second option', value: 'option2' } ]"].Value
                },
            };
        }

        return null;
    }
}
