using System.Text.Json.Nodes;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;
using YesSql.Services;
using ISession = YesSql.ISession;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget;

public sealed class ExportContentToDeploymentTargetDeploymentSource
    : DeploymentSourceBase<ExportContentToDeploymentTargetDeploymentStep>
{
    private readonly ContentExportService _exports;
    private readonly IHttpContextAccessor _httpContext;
    private readonly ISession _session;
    private readonly IUpdateModelAccessor _updateModelAccessor;

    public ExportContentToDeploymentTargetDeploymentSource(
        ISession session,
        IUpdateModelAccessor updateModelAccessor,
        ContentExportService exports,
        IHttpContextAccessor httpContext)
    {
        _exports = exports;
        _httpContext = httpContext;
        _session = session;
        _updateModelAccessor = updateModelAccessor;
    }

    protected override async Task ProcessAsync(ExportContentToDeploymentTargetDeploymentStep step, DeploymentPlanResult result)
    {
        var principal = result.User ?? _httpContext.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
        var selected = new List<(string Id, bool Latest)>();
        if (step.ContentItemIds is not null)
        {
            if (step.ContentItemIds.Length is 0 or > 200 || step.ContentItemIds.Any(string.IsNullOrWhiteSpace))
            {
                throw new InvalidDataException("Explicit content export selection must contain between 1 and 200 IDs.");
            }
            selected.AddRange(step.ContentItemIds.Distinct(StringComparer.Ordinal).Select(id => (id, step.Latest)));
        }
        else
        {
            var model = new ExportContentToDeploymentTargetModel();
            if (_updateModelAccessor.ModelUpdater is null)
            {
                throw new InvalidDataException("This export step requires an explicit selection outside the admin request.");
            }
            await _updateModelAccessor.ModelUpdater.TryUpdateModelAsync(model, "ExportContentToDeploymentTarget", m => m.ItemIds, m => m.Latest, m => m.ContentItemId);
            if (!string.IsNullOrEmpty(model.ContentItemId)) { selected.Add((model.ContentItemId, model.Latest)); }
            if (model.ItemIds?.Any() == true)
            {
                var items = await _session.Query<ContentItem, ContentItemIndex>().Where(item => item.DocumentId.IsIn(model.ItemIds) && item.Published).ListAsync();
                selected.AddRange(items.Select(item => (item.ContentItemId, false)));
            }
        }
        if (selected.Count == 0) { throw new InvalidDataException("Select at least one content item to export."); }
        var data = new JsonArray();
        foreach (var (id, latest) in selected)
        {
            var item = await _exports.GetAsync(id, latest, principal)
                ?? throw new InvalidDataException("A selected content version no longer exists.");
            data.Add(ContentExportService.Serialize(item, recipe: true));
        }
        result.Steps.Add(new JsonObject { ["name"] = "Content", ["data"] = data });
    }

    public class ExportContentToDeploymentTargetModel
    {
        public IEnumerable<long> ItemIds { get; set; }
        public string ContentItemId { get; set; }
        public bool Latest { get; set; }
    }
}
