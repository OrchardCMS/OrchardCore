using System.Text.Json.Nodes;
using Json.Schema;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Settings.Recipes;

/// <summary>
/// Unified recipe/deployment step for importing and exporting site settings.
/// </summary>
public sealed class UnifiedSettingsStep : RecipeDeploymentStep<UnifiedSettingsStep.SettingsStepModel>
{
    private readonly ISiteService _siteService;

    public UnifiedSettingsStep(ISiteService siteService)
    {
        _siteService = siteService;
    }

    /// <inheritdoc />
    public override string Name => "Settings";

    /// <inheritdoc />
    public override string DisplayName => "Settings";

    /// <inheritdoc />
    public override string Description => "Updates site settings for the Orchard Core application.";

    /// <inheritdoc />
    public override string Category => "Configuration";

    /// <inheritdoc />
    protected override JsonSchema BuildSchema()
    {
        return new JsonSchemaBuilder()
            .Schema(MetaSchemas.Draft202012Id)
            .Type(SchemaValueType.Object)
            .Title(Name)
            .Description(Description)
            .Required("name")
            .Properties(
                ("name", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("BaseUrl", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Description("The base URL of the site.")),
                ("Calendar", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Description("The calendar system to use.")),
                ("MaxPagedCount", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Integer)
                    .Description("Maximum number of items to return in paged results.")),
                ("MaxPageSize", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Integer)
                    .Description("Maximum page size allowed.")),
                ("PageSize", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Integer)
                    .Description("Default page size.")),
                ("ResourceDebugMode", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Enum("Disabled", "FromConfiguration", "Enabled")
                    .Description("Resource debug mode setting.")),
                ("SiteName", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Description("The name of the site.")),
                ("PageTitleFormat", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Description("Format string for page titles.")),
                ("SuperUser", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Description("The user ID of the super user.")),
                ("TimeZoneId", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Description("The time zone ID for the site.")),
                ("UseCdn", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Boolean)
                    .Description("Whether to use CDN for resources.")),
                ("CdnBaseUrl", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Description("Base URL for CDN resources.")),
                ("AppendVersion", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Boolean)
                    .Description("Whether to append version to resource URLs.")),
                ("HomeRoute", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Object)
                    .Description("Route values for the home page.")
                    .AdditionalProperties(JsonSchema.Empty)),
                ("CacheMode", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Enum("Disabled", "FromConfiguration", "Enabled")
                    .Description("Cache mode setting.")))
            .AdditionalProperties(JsonSchema.Empty)
            .Build();
    }

    /// <inheritdoc />
    protected override async Task ImportAsync(SettingsStepModel model, RecipeExecutionContext context)
    {
        var site = await _siteService.LoadSiteSettingsAsync();
        site ??= new SiteSettings();

        if (model.BaseUrl is not null)
        {
            site.BaseUrl = model.BaseUrl;
        }

        if (model.Calendar is not null)
        {
            site.Calendar = model.Calendar;
        }

        if (model.MaxPagedCount.HasValue)
        {
            site.MaxPagedCount = model.MaxPagedCount.Value;
        }

        if (model.MaxPageSize.HasValue)
        {
            site.MaxPageSize = model.MaxPageSize.Value;
        }

        if (model.PageSize.HasValue)
        {
            site.PageSize = model.PageSize.Value;
        }

        if (model.ResourceDebugMode.HasValue)
        {
            site.ResourceDebugMode = model.ResourceDebugMode.Value;
        }

        if (model.SiteName is not null)
        {
            site.SiteName = model.SiteName;
        }

        if (model.PageTitleFormat is not null)
        {
            site.PageTitleFormat = model.PageTitleFormat;
        }

        if (model.SiteSalt is not null)
        {
            site.SiteSalt = model.SiteSalt;
        }

        if (model.SuperUser is not null)
        {
            site.SuperUser = model.SuperUser;
        }

        if (model.TimeZoneId is not null)
        {
            site.TimeZoneId = model.TimeZoneId;
        }

        if (model.UseCdn.HasValue)
        {
            site.UseCdn = model.UseCdn.Value;
        }

        if (model.CdnBaseUrl is not null)
        {
            site.CdnBaseUrl = model.CdnBaseUrl;
        }

        if (model.AppendVersion.HasValue)
        {
            site.AppendVersion = model.AppendVersion.Value;
        }

        if (model.HomeRoute is not null)
        {
            site.HomeRoute = model.HomeRoute;
        }

        if (model.CacheMode.HasValue)
        {
            site.CacheMode = model.CacheMode.Value;
        }

        // Handle additional properties.
        if (model.AdditionalProperties is not null)
        {
            foreach (var kvp in model.AdditionalProperties)
            {
                site.Properties[kvp.Key] = kvp.Value?.DeepClone();
            }
        }

        await _siteService.UpdateSiteSettingsAsync(site);
    }

    /// <inheritdoc />
    protected override async Task<SettingsStepModel> BuildExportModelAsync(RecipeExportContext context)
    {
        var site = await _siteService.GetSiteSettingsAsync();

        return new SettingsStepModel
        {
            BaseUrl = site.BaseUrl,
            Calendar = site.Calendar,
            MaxPagedCount = site.MaxPagedCount,
            MaxPageSize = site.MaxPageSize,
            PageSize = site.PageSize,
            ResourceDebugMode = site.ResourceDebugMode,
            SiteName = site.SiteName,
            PageTitleFormat = site.PageTitleFormat,
            SuperUser = site.SuperUser,
            TimeZoneId = site.TimeZoneId,
            UseCdn = site.UseCdn,
            CdnBaseUrl = site.CdnBaseUrl,
            AppendVersion = site.AppendVersion,
            HomeRoute = site.HomeRoute,
            CacheMode = site.CacheMode,
        };
    }

    /// <summary>
    /// Model for the Settings step data.
    /// </summary>
    public sealed class SettingsStepModel
    {
        public string Name { get; set; }
        public string BaseUrl { get; set; }
        public string Calendar { get; set; }
        public int? MaxPagedCount { get; set; }
        public int? MaxPageSize { get; set; }
        public int? PageSize { get; set; }
        public ResourceDebugMode? ResourceDebugMode { get; set; }
        public string SiteName { get; set; }
        public string PageTitleFormat { get; set; }
        public string SiteSalt { get; set; }
        public string SuperUser { get; set; }
        public string TimeZoneId { get; set; }
        public bool? UseCdn { get; set; }
        public string CdnBaseUrl { get; set; }
        public bool? AppendVersion { get; set; }
        public RouteValueDictionary HomeRoute { get; set; }
        public CacheMode? CacheMode { get; set; }

        public JsonObject AdditionalProperties { get; set; }
    }
}
