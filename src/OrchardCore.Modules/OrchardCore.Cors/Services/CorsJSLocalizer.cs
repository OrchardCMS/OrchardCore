using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;

namespace OrchardCore.Cors.Services;

public sealed class CorsJSLocalizer(IStringLocalizer<CorsJSLocalizer> S) : IJSLocalizer
{
    public IDictionary<string, string> GetLocalizations(string group)
    {
        if (string.Equals(group, "cors-admin", StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>
            {
                { "Search", S["Search"].Value },
                { "AddPolicy", S["Add a policy"].Value },
                { "DefaultPolicy", S["Default Policy"].Value },
                { "Edit", S["Edit"].Value },
                { "Delete", S["Delete"].Value },
                { "Add", S["Add"].Value },
                { "Save", S["Save"].Value },
                { "Cancel", S["Cancel"].Value },
                { "NothingHere", S["Nothing here! There are no CORS policies for the moment."].Value },
                { "NothingHereSearch", S["Nothing here! Your search returned no results."].Value },
                { "Details", S["Details"].Value },
                { "ProvidePolicyDetails", S["Provide policy details."].Value },
                { "PolicyName", S["Policy name"].Value },
                { "PolicyNameHint", S["The name of the policy."].Value },
                { "SetAsDefaultPolicy", S["Set as default policy"].Value },
                { "Credentials", S["Credentials"].Value },
                { "ConfigureCredentialsBehavior", S["Configure the credentials behavior."].Value },
                { "AllowCredentials", S["Allow credentials"].Value },
                { "AllowCredentialsHint", S["Allows credentials to be transported in requests. This setting is not allowed in combination with Allow Any Origin."].Value },
                { "Origins", S["Origins"].Value },
                { "ConfigureOriginsBehavior", S["Configure the origins behavior."].Value },
                { "AllowAnyOrigin", S["Allow any origin"].Value },
                { "AllowAnyOriginHint", S["Allows requests from any origin. This will bypass any origin that is configured below."].Value },
                { "Origin", S["Origin"].Value },
                { "AllowedOrigins", S["Allowed origins"].Value },
                { "Headers", S["Headers"].Value },
                { "AllowHeadersHint", S["Allow certain or all headers to be used by the external client."].Value },
                { "AllowAnyHeader", S["Allow any header"].Value },
                { "AllowAnyHeaderHint", S["Allows requests with any header. This will bypass any headers that are configured below."].Value },
                { "Header", S["Header"].Value },
                { "AllowedHeaders", S["Allowed headers"].Value },
                { "Methods", S["Methods"].Value },
                { "ConfigureMethodsBehavior", S["Configure the methods behavior (POST, PUT, DELETE, GET, etc)."].Value },
                { "AllowAnyMethod", S["Allow any method"].Value },
                { "AllowAnyMethodHint", S["Allows requests with any method. This will bypass any methods that are configured below."].Value },
                { "Method", S["Method"].Value },
                { "AllowedMethods", S["Allowed methods"].Value },
                { "ExposedHeaders", S["Exposed headers"].Value },
                { "ConfigureExposedHeaders", S["Configure which headers should be exposed."].Value },
                { "ExposedHeadersHint", S["Sets response header 'Access-Control-Expose-Headers'."].Value },
            };
        }

        return null;
    }
}
