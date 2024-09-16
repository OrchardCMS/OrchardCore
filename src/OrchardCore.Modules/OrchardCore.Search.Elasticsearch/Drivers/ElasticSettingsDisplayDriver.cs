using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Nest;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Search.Elasticsearch.Drivers;

public sealed class ElasticSettingsDisplayDriver : SiteDisplayDriver<ElasticSettings>
{
    private static readonly char[] _separator = [',', ' '];

    private readonly ElasticIndexSettingsService _elasticIndexSettingsService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly ElasticConnectionOptions _elasticConnectionOptions;
    private readonly IElasticClient _elasticClient;

    internal readonly IStringLocalizer S;

    protected override string SettingsGroupId
        => SearchConstants.SearchSettingsGroupId;

    public ElasticSettingsDisplayDriver(
        ElasticIndexSettingsService elasticIndexSettingsService,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IOptions<ElasticConnectionOptions> elasticConnectionOptions,
        IElasticClient elasticClient,
        IStringLocalizer<ElasticSettingsDisplayDriver> stringLocalizer
        )
    {
        _elasticIndexSettingsService = elasticIndexSettingsService;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _elasticConnectionOptions = elasticConnectionOptions.Value;
        _elasticClient = elasticClient;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ISite site, ElasticSettings settings, BuildEditorContext context)
        => Initialize<ElasticSettingsViewModel>("ElasticSettings_Edit", async model =>
        {
            model.SearchIndex = settings.SearchIndex;
            model.SearchFields = string.Join(", ", settings.DefaultSearchFields ?? []);
            model.SearchIndexes = (await _elasticIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName);
            model.DefaultQuery = settings.DefaultQuery;
            model.SearchType = settings.GetSearchType();
            model.SearchTypes = [
                new(S["Multi-Match Query (Default)"], string.Empty),
                new(S["Query String Query"], ElasticSettings.QueryStringSearchType),
                new(S["Custom Query"], ElasticSettings.CustomSearchType),
            ];
        }).Location("Content:2#Elasticsearch;10")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Permissions.ManageElasticIndexes))
        .OnGroup(SettingsGroupId);

    public override async Task<IDisplayResult> UpdateAsync(ISite site, ElasticSettings section, UpdateEditorContext context)
    {
        if (!_elasticConnectionOptions.FileConfigurationExists())
        {
            return null;
        }

        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, Permissions.ManageElasticIndexes))
        {
            return null;
        }

        var model = new ElasticSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        section.DefaultQuery = null;
        section.SearchIndex = model.SearchIndex;
        section.DefaultSearchFields = model.SearchFields?.Split(_separator, StringSplitOptions.RemoveEmptyEntries);
        section.SearchType = model.SearchType ?? string.Empty;

        if (model.SearchType == ElasticSettings.CustomSearchType)
        {
            if (string.IsNullOrWhiteSpace(model.DefaultQuery))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.DefaultQuery), S["Please provide the default query."]);
            }
            else if (!JObject.TryParse(model.DefaultQuery, out var jsonObject))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.DefaultQuery), S["The provided query is not formatted correctly."]);
            }
            else
            {
                section.DefaultQuery = jsonObject.ToJsonString(JOptions.Indented);
                try
                {
                    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(model.DefaultQuery));
                    var searchRequest = await _elasticClient.RequestResponseSerializer.DeserializeAsync<SearchRequest>(stream);
                }
                catch
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.DefaultQuery), S["Invalid query provided."]);
                }
            }
        }

        return Edit(site, section, context);
    }
}
