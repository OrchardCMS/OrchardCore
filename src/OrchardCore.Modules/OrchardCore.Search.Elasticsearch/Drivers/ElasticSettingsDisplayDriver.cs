using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
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

public class ElasticSettingsDisplayDriver : SectionDisplayDriver<ISite, ElasticSettings>
{
    public const string GroupId = "elasticsearch";

    private static readonly char[] _separator = [',', ' '];
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
    };
    private readonly ElasticIndexSettingsService _elasticIndexSettingsService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IElasticClient _elasticClient;
    protected readonly IStringLocalizer S;

    public ElasticSettingsDisplayDriver(
        ElasticIndexSettingsService elasticIndexSettingsService,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IElasticClient elasticClient,
        IStringLocalizer<ElasticSettingsDisplayDriver> stringLocalizer
        )
    {
        _elasticIndexSettingsService = elasticIndexSettingsService;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _elasticClient = elasticClient;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ElasticSettings settings)
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
        }).Location("Content:2")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Permissions.ManageElasticIndexes))
        .OnGroup(GroupId);

    public override async Task<IDisplayResult> UpdateAsync(ElasticSettings section, BuildEditorContext context)
    {
        if (!string.Equals(GroupId, context.GroupId, StringComparison.OrdinalIgnoreCase))
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
            else if (!JsonHelpers.TryParse(model.DefaultQuery, out var document))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.DefaultQuery), S["The provided query is not formatted correctly."]);
            }
            else
            {
                section.DefaultQuery = JsonSerializer.Serialize(document, _jsonSerializerOptions);

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

        return await EditAsync(section, context);
    }
}
