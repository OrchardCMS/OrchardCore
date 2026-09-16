using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Settings.Endpoints.Api;

internal static class SiteSettingsSectionEndpoints
{
    internal const string Capability = "settings-sections";

    public static void AddSiteSettingsSectionEndpoints(this IEndpointRouteBuilder routes)
    {
        Configure(routes.MapGet("api/settings/sections", ListAsync), "ApiListSiteSettingsSections", "Lists authorized settings sections from enabled features.", "list")
            .Produces<SiteSettingsSectionDescriptor[]>();
        Configure(routes.MapGet("api/settings/sections/{name}", GetAsync), "ApiGetSiteSettingsSection", "Reads safe values and ownership for an explicit settings section.", "show", argument: true)
            .Produces<SiteSettingsSectionResponse>().ProducesProblem(404);
        Configure(routes.MapGet("api/settings/sections/{name}/schema", SchemaAsync), "ApiGetSiteSettingsSectionSchema", "Gets the explicit update schema for a settings section.", "schema", argument: true)
            .Produces<JsonObject>().ProducesProblem(404);
        Configure(routes.MapPut("api/settings/sections/{name}", UpdateAsync), "ApiUpdateSiteSettingsSection", "Updates supplied settings fields; omitted properties retain their values.", "update", argument: true, input: true)
            .Accepts<JsonObject>("application/json").Produces<SiteSettingsSectionUpdateResult>().ProducesProblem(404).ProducesProblem(409);
    }

    private static RouteHandlerBuilder Configure(RouteHandlerBuilder builder, string id, string summary, string verb, bool argument = false, bool input = false)
    {
        var metadata = new CliOperationMetadata(["settings", "sections"], verb)
        {
            Capability = Capability,
            InputMode = input ? CliInputMode.Json : CliInputMode.Options,
        };
        if (argument)
        {
            metadata.Arguments.Add(new CliArgumentMetadata("name", 0));
        }
        return builder.WithName(id).WithTags("Settings Sections").WithSummary(summary).WithCliCommand(metadata)
            .DisableAntiforgery().RequireAuthorization(policy => policy
                .AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api).RequireAuthenticatedUser()
                .AddRequirements(new OrchardCore.Security.PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement)))
            .ProducesProblem(400).ProducesProblem(401).ProducesProblem(403);
    }

    internal static async Task<IResult> ListAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IEnumerable<ISiteSettingsSectionProvider> providers)
    {
        if (!await HasAccessAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        var descriptors = new List<SiteSettingsSectionDescriptor>();
        foreach (var provider in Unique(providers))
        {
            if (await authorization.AuthorizeAsync(context.User, provider.ReadPermission))
            {
                descriptors.Add(provider.Descriptor);
            }
        }
        return TypedResults.Ok(descriptors.OrderBy(descriptor => descriptor.Name, StringComparer.Ordinal).ToArray());
    }

    internal static async Task<IResult> GetAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IEnumerable<ISiteSettingsSectionProvider> providers, string name) =>
        await ReadAsync(context, authorization, providers, name, schema: false);

    internal static async Task<IResult> SchemaAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IEnumerable<ISiteSettingsSectionProvider> providers, string name) =>
        await ReadAsync(context, authorization, providers, name, schema: true);

    private static async Task<IResult> ReadAsync(HttpContext context, IAuthorizationService authorization,
        IEnumerable<ISiteSettingsSectionProvider> providers, string name, bool schema)
    {
        if (!await HasAccessAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        var provider = Find(providers, name);
        if (provider is null)
        {
            return context.ApiNotFoundProblem();
        }
        if (!await authorization.AuthorizeAsync(context.User, provider.ReadPermission))
        {
            return context.ApiForbidProblem();
        }
        return schema ? TypedResults.Ok(provider.GetSchema()) : TypedResults.Ok(await provider.GetAsync());
    }

    internal static async Task<IResult> UpdateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IEnumerable<ISiteSettingsSectionProvider> providers, string name, [FromBody] JsonObject values)
    {
        if (!await HasAccessAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        var provider = Find(providers, name);
        if (provider is null)
        {
            return context.ApiNotFoundProblem();
        }
        // Update returns readback, so it requires both read and write permission.
        if (!await authorization.AuthorizeAsync(context.User, provider.ReadPermission)
            || !await authorization.AuthorizeAsync(context.User, provider.UpdatePermission))
        {
            return context.ApiForbidProblem();
        }
        if (provider.Descriptor.RequiresHttps && !context.Request.IsHttps)
        {
            return TypedResults.Problem("This settings section can only be changed over HTTPS.", statusCode: 403);
        }
        if (values is null)
        {
            return TypedResults.Problem("A settings object is required.", statusCode: 400);
        }
        var current = await provider.GetAsync();
        if (current.IsReadOnly || values.Any(entry => current.ReadOnlyProperties.Contains(entry.Key, StringComparer.Ordinal)))
        {
            return TypedResults.Problem("The section or a supplied property is controlled by configuration and cannot be changed here.", statusCode: 409);
        }
        var result = await provider.UpdateAsync(values);
        return result.Errors.Count > 0 ? TypedResults.ValidationProblem(result.Errors) : TypedResults.Ok(result);
    }

    private static Task<bool> HasAccessAsync(HttpContext context, IAuthorizationService authorization) =>
        authorization.AuthorizeAsync(context.User, RemoteManagementPermissions.AccessRemoteManagement);

    private static ISiteSettingsSectionProvider Find(IEnumerable<ISiteSettingsSectionProvider> providers, string name) =>
        Unique(providers).SingleOrDefault(provider => string.Equals(provider.Descriptor.Name, name, StringComparison.OrdinalIgnoreCase));

    private static ISiteSettingsSectionProvider[] Unique(IEnumerable<ISiteSettingsSectionProvider> providers)
    {
        var entries = providers.ToArray();
        if (entries.Any(provider => string.IsNullOrWhiteSpace(provider.Descriptor.Name))
            || entries.DistinctBy(provider => provider.Descriptor.Name, StringComparer.OrdinalIgnoreCase).Count() != entries.Length)
        {
            throw new InvalidOperationException("Settings section providers must have unique, nonempty names.");
        }
        return entries;
    }
}
