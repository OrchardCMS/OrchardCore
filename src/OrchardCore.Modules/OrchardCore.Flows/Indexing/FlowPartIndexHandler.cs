using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Flows.Models;
using OrchardCore.Indexing;

namespace OrchardCore.Flows.Indexing
{
    public class FlowPartIndexHandler : ContentPartIndexHandler<FlowPart>
    {
        private readonly IServiceProvider _serviceProvider;

        public FlowPartIndexHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override async Task BuildIndexAsync(FlowPart FlowPart, BuildPartIndexContext context)
        {
            var options = context.Settings.ToOptions();
            if (options == DocumentIndexOptions.None)
            {
                return;
            }

            if (FlowPart.Widgets.Count != 0)
            {
                // Lazy resolution to prevent cyclic dependency.
                var contentItemIndexHandlers = _serviceProvider.GetServices<IContentItemIndexHandler>();

                foreach (var contentItemIndexHandler in contentItemIndexHandlers)
                {
                    foreach (var contentItem in FlowPart.Widgets)
                    {
                        var keys = new List<string>
                        {
                            contentItem.ContentType,
                        };

                        foreach (var key in context.Keys)
                        {
                            keys.Add($"{key}.{contentItem.ContentType}");
                        }

                        var buildIndexContext = new BuildIndexContext(context.DocumentIndex, contentItem, keys, context.Settings);

                        await contentItemIndexHandler.BuildIndexAsync(buildIndexContext);
                    }
                }
            }
        }
    }
}
