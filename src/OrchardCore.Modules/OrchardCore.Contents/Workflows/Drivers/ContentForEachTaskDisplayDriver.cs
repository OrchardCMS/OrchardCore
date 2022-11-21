using OrchardCore.Workflows.Display;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.ContentManagement.Metadata;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Workflows.Models;
using OrchardCore.Queries;
using System.Collections.Generic;
using YesSql;
using System.Threading.Tasks;

namespace OrchardCore.Contents.Workflows.Drivers
{


    public class ContentForEachTaskDisplayDriver: ActivityDisplayDriver<ContentForEachTask, ContentForEachTaskViewModel>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IQueryManager _queryManager;
        private readonly ISession _session;
        public ContentForEachTaskDisplayDriver(IContentDefinitionManager contentDefinitionManager, IQueryManager queryManager, ISession session)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _queryManager = queryManager;
            _session = session;
        }
        protected override void EditActivity(ContentForEachTask activity, ContentForEachTaskViewModel model)
        {
            model.UseQuery = activity.UseQuery;
            model.Query = activity.Query;
            model.Parameters = activity.Parameters;
            model.AvailableContentTypes = _contentDefinitionManager.ListTypeDefinitions()
                .Select(x => new SelectListItem { Text = x.DisplayName, Value = x.Name })
                .ToList();
            var task = _queryManager.ListQueriesAsync();
            var queries = task.Result as List<Query>;
            model.Queries = queries.Where(w => w.Source != "Sql").Select(x => new SelectListItem { Text = x.Name, Value = x.Name }).ToList();
            model.ContentType = activity.ContentType;
            model.Take = activity.Take;
            model.PublishedOnly = activity.PublishedOnly;
        }

        protected override void UpdateActivity(ContentForEachTaskViewModel model, ContentForEachTask activity)
        {
            activity.UseQuery = model.UseQuery;
            activity.ContentType = model.ContentType;
            activity.Query = model.Query;
            activity.Parameters = model.Parameters ?? string.Empty;
            activity.Take = model.Take;
            activity.PublishedOnly = model.PublishedOnly;
        }
    }

}
