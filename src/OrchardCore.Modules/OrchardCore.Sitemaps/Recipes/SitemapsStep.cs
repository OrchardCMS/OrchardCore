using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Recipes
{
    /// <summary>
    /// This recipe step creates a set of sitemaps.
    /// </summary>
    public class SitemapsStep : IRecipeStepHandler
    {
        private static readonly JsonSerializer _serializer = new()
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        private readonly ISitemapManager _sitemapManager;

        public SitemapsStep(ISitemapManager sitemapManager)
        {
            _sitemapManager = sitemapManager;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "Sitemaps", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<SitemapStepModel>();

            foreach (var token in model.Data.Cast<JObject>())
            {
                var sitemap = token.ToObject<SitemapType>(_serializer);
                await _sitemapManager.UpdateSitemapAsync(sitemap);
            }
        }

        public class SitemapStepModel
        {
            public JArray Data { get; set; }
        }
    }
}
