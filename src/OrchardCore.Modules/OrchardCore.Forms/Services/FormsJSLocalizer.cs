using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;

namespace OrchardCore.Forms.Services;

public sealed class FormsJSLocalizer(IStringLocalizer<FormsJSLocalizer> S) : IJSLocalizer
{
    public IDictionary<string, string> GetLocalizations(string group)
    {
        if (string.Equals(group, "forms-select-part-editor", StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>
            {
                { "EnterTheText", S["Enter the text"].Value },
                { "EnterAValue", S["Enter a value"].Value },
                { "SetAsDefault", S["Set as default"].Value },
                { "RemoveElementFromList", S["Remove element from list"].Value },
                { "OptionText", S["Option Text"].Value },
                { "Value", S["Value"].Value },
                { "Default", S["Default?"].Value },
                { "AddAnOption", S["Add an option"].Value },
                { "EditData", S["Edit Data"].Value },
                { "Options", S["Options"].Value },
                { "JsonRepresentationHint", S["A JSON representation of the allowed values, e.g. {0}", "[ { text: 'First option', value: 'option1' }, { text: 'Second option', value: 'option2' } ]"].Value },
                { "DefaultValue", S["Default value"].Value },
                { "DefaultValueHint", S["(Optional) The value to assign to the select field."].Value },
                { "Ok", S["OK"].Value },
                { "Cancel", S["Cancel"].Value },
            };
        }

        return null;
    }
}
