using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Queries;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using System;
using System.Linq;
using YesSql;


namespace OrchardCore.Contents.Workflows.Drivers
{
    public class ContentForEachTaskDisplayDriver : ActivityDisplayDriver<ContentForEachTask, ContentForEachTaskViewModel>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IServiceProvider _serviceProvider;
        public ContentForEachTaskDisplayDriver(IContentDefinitionManager contentDefinitionManager, ISession session, ShellDescriptor shellDescriptor, IServiceProvider serviceProvider)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _shellDescriptor = shellDescriptor;
            _serviceProvider = serviceProvider;
        }
        protected override async void EditActivity(ContentForEachTask activity, ContentForEachTaskViewModel model)
        {
            model.QueriesEnabled = _shellDescriptor.Features.Any(feature => feature.Id == "OrchardCore.Queries");
            if (model.QueriesEnabled)
            {
                var _queryManager = (IQueryManager)_serviceProvider.GetService(typeof(IQueryManager));
                model.UseQuery = activity.UseQuery;
                model.QuerySource = activity.QuerySource;
                model.Query = activity.Query;
                model.Parameters = activity.Parameters?.Expression ?? "";
                var queries = await _queryManager.ListQueriesAsync();

                model.QuerySources = queries.Select(x => x.Source).Distinct()
                    .Select(x => new SelectListItem { Text = x, Value = x })
                    .ToList();

                model.QueriesBySource = queries.GroupBy(x => x.Source)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(q => new SelectListItem { Text = q.Name, Value = q.Name }).ToList());

            }
            else
            {
                model.UseQuery = false;
            }

            model.AvailableContentTypes = (await _contentDefinitionManager.ListTypeDefinitionsAsync())
                .Select(x => new SelectListItem { Text = x.DisplayName, Value = x.Name })
                .ToList();
            model.ContentType = activity.ContentType;
            model.PageSize = activity.PageSize;
            model.PublishedOnly = activity.PublishedOnly;
        }

        protected override void UpdateActivity(ContentForEachTaskViewModel model, ContentForEachTask activity)
        {
            activity.UseQuery = model.UseQuery;
            activity.ContentType = model.ContentType;
            activity.Query = model.Query ?? string.Empty;
            activity.QuerySource = model.QuerySource;
            activity.Parameters = new WorkflowExpression<string>(model.Parameters);
            activity.PageSize = model.PageSize;
            activity.PublishedOnly = model.PublishedOnly;
        }
    }

}
