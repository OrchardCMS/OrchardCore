using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Feeds.Models;

namespace OrchardCore.Feeds.Controllers
{
    public class FeedController : Controller, IUpdateModel
    {
        private readonly IEnumerable<IFeedBuilderProvider> _feedFormatProviders;
        private readonly IEnumerable<IFeedQueryProvider> _feedQueryProviders;
        private readonly IFeedItemBuilder _feedItemBuilder;
        private readonly IServiceProvider _serviceProvider;

        public FeedController(
            IEnumerable<IFeedQueryProvider> feedQueryProviders,
            IEnumerable<IFeedBuilderProvider> feedFormatProviders,
            IFeedItemBuilder feedItemBuilder,
            ILogger<FeedController> logger,
            IServiceProvider serviceProvider)
        {
            _feedQueryProviders = feedQueryProviders;
            _feedFormatProviders = feedFormatProviders;
            _feedItemBuilder = feedItemBuilder;
            _serviceProvider = serviceProvider;
            Logger = logger;
        }

        ILogger Logger { get; }

        public async Task<ActionResult> Index(string format)
        {
            var context = new FeedContext(this, format);

            var bestFormatterMatch = _feedFormatProviders
                .Select(provider => provider.Match(context))
                .Where(match => match != null && match.FeedBuilder != null)
                .OrderByDescending(match => match.Priority)
                .FirstOrDefault();

            if (bestFormatterMatch == null || bestFormatterMatch.FeedBuilder == null)
            {
                return NotFound();
            }

            context.Builder = bestFormatterMatch.FeedBuilder;

            var bestQueryMatch = _feedQueryProviders
                .Select(provider => provider.Match(context))
                .Where(match => match != null && match.FeedQuery != null)
                .OrderByDescending(match => match.Priority)
                .FirstOrDefault();

            if (bestQueryMatch == null || bestQueryMatch.FeedQuery == null)
            {
                return NotFound();
            }

            var document = await context.Builder.ProcessAsync(context, async () =>
           {
               await bestQueryMatch.FeedQuery.ExecuteAsync(context);

               await _feedItemBuilder.PopulateAsync(context);

               foreach (var contextualizer in context.Response.Contextualizers)
               {
                   if (ControllerContext != null)
                   {
                       contextualizer(new ContextualizeContext
                       {
                           ServiceProvider = _serviceProvider,
                           Url = Url
                       });
                   }
               }
           });

            return Content(document.ToString(), "text/xml");
        }
    }
}
