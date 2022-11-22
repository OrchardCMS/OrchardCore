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
using OrchardCore.Environment.Shell.Descriptor.Models;
using System;

namespace OrchardCore.Contents.Workflows.Drivers
{


    public class ContentForEachTaskDisplayDriver: ActivityDisplayDriver<ContentForEachTask, ContentForEachTaskViewModel>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISession _session;
        private readonly ShellDescriptor _shellDescriptor; 
        private readonly IServiceProvider _serviceProvider;
        public ContentForEachTaskDisplayDriver(IContentDefinitionManager contentDefinitionManager, ISession session, ShellDescriptor shellDescriptor, IServiceProvider serviceProvider)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _session = session;
            _shellDescriptor = shellDescriptor;
            _serviceProvider = serviceProvider;
        }
        protected override void EditActivity(ContentForEachTask activity, ContentForEachTaskViewModel model)
        {
            model.QueriesEnabled = _shellDescriptor.Features.Any(feature => feature.Id == "OrchardCore.Queries");
            if(model.QueriesEnabled)
            {
                var _queryManager = (IQueryManager)_serviceProvider.GetService(typeof(IQueryManager));
                model.UseQuery = activity.UseQuery;
                model.Query = activity.Query;
                model.Parameters = activity.Parameters;
                var task = _queryManager.ListQueriesAsync();
                var queries = task.Result as List<Query>;
                model.Queries = queries.Where(w => w.Source != "Sql").Select(x => new SelectListItem { Text = x.Name, Value = x.Name }).ToList();
            }
            else
            {
                model.UseQuery = false;
            }

            model.AvailableContentTypes = _contentDefinitionManager.ListTypeDefinitions()
                .Select(x => new SelectListItem { Text = x.DisplayName, Value = x.Name })
                .ToList();
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
