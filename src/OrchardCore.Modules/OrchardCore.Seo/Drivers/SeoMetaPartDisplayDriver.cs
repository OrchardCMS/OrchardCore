using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;
using OrchardCore.Mvc.Utilities;
using OrchardCore.ResourceManagement;
using OrchardCore.Seo.Models;
using OrchardCore.Seo.ViewModels;

namespace OrchardCore.Seo.Drivers
{
    public class SeoMetaPartDisplayDriver : ContentPartDisplayDriver<SeoMetaPart>
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented
        };

        private readonly IContentManager _contentManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILiquidTemplateManager _liquidTemplatemanager;
        private readonly IStringLocalizer S;

        public SeoMetaPartDisplayDriver(
            IContentManager contentManager,
            IServiceProvider serviceProvider,
            ILiquidTemplateManager liquidTemplateManager,
            IStringLocalizer<SeoMetaPartDisplayDriver> stringLocalizer
            )
        {
            _contentManager = contentManager;
            _serviceProvider = serviceProvider;
            _liquidTemplatemanager = liquidTemplateManager;
            S = stringLocalizer;
        }

        public override IDisplayResult Edit(SeoMetaPart part, BuildPartEditorContext context)
        {
            var settings = context.TypePartDefinition.GetSettings<SeoMetaPartSettings>();

            var results = new List<IDisplayResult>();

            results.Add(Initialize<SeoMetaPartViewModel>("SeoMetaPart_Edit", model =>
                {
                    model.PageTitle = part.PageTitle;
                    model.Render = part.Render;
                    model.MetaDescription = part.MetaDescription;
                    model.MetaKeywords = part.MetaKeywords;
                    model.Canonical = part.Canonical;
                    model.MetaRobots = part.MetaRobots;
                    model.CustomMetaTags = JsonConvert.SerializeObject(part.CustomMetaTags, SerializerSettings);
                    model.SeoMetaPart = part;
                    model.Settings = settings;
                }).Location("Parts#SEO;50"));

            if (settings.DisplayOpenGraph)
            {
                results.Add(Initialize<SeoMetaPartOpenGraphViewModel>("SeoMetaPartOpenGraph_Edit", model =>
                {
                    model.OpenGraphType = part.OpenGraphType;
                    model.OpenGraphTitle = part.OpenGraphTitle;
                    model.OpenGraphDescription = part.OpenGraphDescription;
                    model.SeoMetaPart = part;
                }).Location("Parts#SEO;50%Open Graph;20"));
            }

            if (settings.DisplayTwitter)
            {
                results.Add(Initialize<SeoMetaPartTwitterViewModel>("SeoMetaPartTwitter_Edit", model =>
                {
                    model.TwitterTitle = part.TwitterTitle;
                    model.TwitterDescription = part.TwitterDescription;
                    model.TwitterCard = part.TwitterCard;
                    model.TwitterCreator = part.TwitterCreator;
                    model.TwitterSite = part.TwitterSite;
                    model.SeoMetaPart = part;
                }).Location("Parts#SEO;50%Twitter;30"));
            }

            if (settings.DisplayGoogleSchema)
            {
                results.Add(Initialize<SeoMetaPartGoogleSchemaViewModel>("SeoMetaPartGoogleSchema_Edit", model =>
                    {
                        model.GoogleSchema = part.GoogleSchema;
                        model.SeoMetaPart = part;
                    }).Location("Parts#SEO;50%Google Schema;40"));
            }

            return Combine(results);
        }

        public override async Task<IDisplayResult> UpdateAsync(SeoMetaPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var partViewModel = new SeoMetaPartViewModel();
            if (await updater.TryUpdateModelAsync(partViewModel, Prefix))
            {
                try
                {
                    part.Render = partViewModel.Render;
                    part.PageTitle = partViewModel.PageTitle;
                    part.MetaDescription = partViewModel.MetaDescription;
                    part.MetaKeywords = partViewModel.MetaKeywords;
                    part.Canonical = partViewModel.Canonical;
                    part.MetaRobots = partViewModel.MetaRobots;

                    part.CustomMetaTags = String.IsNullOrWhiteSpace(partViewModel.CustomMetaTags)
                        ? Array.Empty<MetaEntry>()
                        : JsonConvert.DeserializeObject<MetaEntry[]>(partViewModel.CustomMetaTags);

                    if (part.Canonical?.IndexOfAny(SeoMetaPart.InvalidCharactersForCanoncial) > -1 || part.Canonical?.IndexOf(' ') > -1)
                    {
                        updater.ModelState.AddModelError(Prefix, S["The canonical entry contains invalid characters."]);
                    }
                }
                catch
                {
                    updater.ModelState.AddModelError(Prefix, S["The meta entries are written in an incorrect format."]);
                }
            }

            var openGraphModel = new SeoMetaPartOpenGraphViewModel();
            if (await updater.TryUpdateModelAsync(openGraphModel, Prefix))
            {
                part.OpenGraphType = openGraphModel.OpenGraphType;
                part.OpenGraphTitle = openGraphModel.OpenGraphTitle;
                part.OpenGraphDescription = openGraphModel.OpenGraphDescription;
            }

            var twitterModel = new SeoMetaPartTwitterViewModel();
            if (await updater.TryUpdateModelAsync(twitterModel, Prefix))
            {
                part.TwitterTitle = twitterModel.TwitterTitle;
                part.TwitterDescription = twitterModel.TwitterDescription;
                part.TwitterCard = twitterModel.TwitterCard;
                part.TwitterCreator = twitterModel.TwitterCreator;
                part.TwitterSite = twitterModel.TwitterSite;
            }

            var googleSchemaModel = new SeoMetaPartGoogleSchemaViewModel();
            if (await updater.TryUpdateModelAsync(googleSchemaModel, Prefix))
            {
                part.GoogleSchema = googleSchemaModel.GoogleSchema;
                if (!String.IsNullOrWhiteSpace(googleSchemaModel.GoogleSchema) && !googleSchemaModel.GoogleSchema.IsJson())
                {
                    updater.ModelState.AddModelError(Prefix, S["The google schema is written in an incorrect format."]);
                }
            }

            return Edit(part, context);
        }
    }
}
