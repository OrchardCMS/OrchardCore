using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Metadata.Models;
using OrchardCore.Metadata.ViewModels;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.Metadata.Drivers
{
    public class MetadataPartDisplayDriver : ContentPartDisplayDriver<MetadataPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISiteService _siteService;
        private readonly IResourceManager _resourceManager;

        public MetadataPartDisplayDriver(
            IContentDefinitionManager contentDefinitionManager,
            ISiteService siteService, IResourceManager resourceManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _siteService = siteService;
            _resourceManager = resourceManager;
        }

        public override IDisplayResult Display(MetadataPart metadataPart)
        {
            return Initialize<MetadataPartViewModel>("MetadataPart", async model => await BuildViewModelAsync(model, metadataPart))
            .Location("Detail", "Content:20");
        }

        public override async Task<IDisplayResult> DisplayAsync(MetadataPart part, BuildPartDisplayContext context)
        {

            var viewModel = new MetadataPartViewModel();
            await BuildViewModelAsync(viewModel, part);

            var settings = GetSettings(part);

            if (context.DisplayType == "Detail")
            {

                if (!String.IsNullOrWhiteSpace(viewModel.MetaDescription))
                {
                    _resourceManager.RegisterMeta(new MetaEntry
                    {
                        Name = "description",
                        Content = viewModel.MetaDescription
                    });
                }

                if (settings.SupportMetaKeywords && !String.IsNullOrWhiteSpace(viewModel.MetaKeywords))
                {
                    _resourceManager.RegisterMeta(new MetaEntry
                    {
                        Name = "keywords",
                        Content = viewModel.MetaKeywords
                    });
                }
                // Both Twitter and Facebook share some Open Graph tags so use these if possible rather than the platform specific ones.
                if (settings.SupportOpenGraph || settings.SupportTwitterCards)
                {
                    if (!String.IsNullOrWhiteSpace(viewModel.OpenGraphTitle))
                    {
                        _resourceManager.RegisterMeta(new MetaEntry
                        {
                            Property = "og:title",
                            Content = viewModel.OpenGraphTitle
                        });
                    }
                    if (!String.IsNullOrWhiteSpace(viewModel.OpenGraphDescription))
                    {
                        _resourceManager.RegisterMeta(new MetaEntry
                        {
                            Property = "og:description",
                            Content = viewModel.OpenGraphDescription
                        });
                    }
                    if (!String.IsNullOrWhiteSpace(viewModel.OpenGraphImage))
                    {
                        _resourceManager.RegisterMeta(new MetaEntry
                        {
                            Property = "og:image",
                            Content = viewModel.OpenGraphImage
                        });
                    }
                }
                // Extended metatags for Facebook Open Graph
                if (settings.SupportOpenGraph)
                {
                    if (!String.IsNullOrWhiteSpace(viewModel.OpenGraphType))
                    {
                        _resourceManager.RegisterMeta(new MetaEntry
                        {
                            Property = "og:type",
                            Content = viewModel.OpenGraphType
                        });
                    }
                    if (!String.IsNullOrWhiteSpace(viewModel.OpenGraphUrl))
                    {
                        _resourceManager.RegisterMeta(new MetaEntry
                        {
                            Property = "og:url",
                            Content = viewModel.OpenGraphUrl
                        });
                    }
                    if (!String.IsNullOrWhiteSpace(viewModel.OpenGraphImageAlt) && !String.IsNullOrWhiteSpace(viewModel.OpenGraphImage))
                    {
                        _resourceManager.RegisterMeta(new MetaEntry
                        {
                            Property = "og:image:alt",
                            Content = viewModel.OpenGraphImageAlt
                        });
                    }
                }
                // Extended metatags for Twitter
                if (settings.SupportTwitterCards)
                {
                    if (!String.IsNullOrWhiteSpace(viewModel.TwitterCard))
                    {
                        _resourceManager.RegisterMeta(new MetaEntry
                        {
                            Name = "twitter:card",
                            Content = viewModel.TwitterCard
                        });
                    }
                    // Twitter does not support the Open Graph Image Alt tag so we have to add Twitter's own tag
                    if (!String.IsNullOrWhiteSpace(viewModel.OpenGraphImageAlt) && !String.IsNullOrWhiteSpace(viewModel.OpenGraphImage))
                    {
                        _resourceManager.RegisterMeta(new MetaEntry
                        {
                            Name = "twitter:image:alt",
                            Content = viewModel.OpenGraphImageAlt
                        });
                    }
                }
            }

            // TODO these should be added on every page regardless of whether the Metadata part is added to the content type
            // If an Facebook App id is registered in the site settings
            if (!String.IsNullOrWhiteSpace(viewModel.FacebookAppId))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Property = "fb:app_id",
                    Content = viewModel.FacebookAppId
                });
            }
            // If a Twitter site id is registered in the site settings
            if (!String.IsNullOrWhiteSpace(viewModel.TwitterSite))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Name = "twitter:site",
                    Content = viewModel.TwitterSite
                });
            }

            return await base.DisplayAsync(part, context);
        }

        public override IDisplayResult Edit(MetadataPart metadataPart)
        {
            return Initialize<MetadataPartViewModel>("MetadataPart_Edit", model =>
            {
                var settings = GetSettings(metadataPart);

                model.MetaTitle = metadataPart.MetaTitle;
                model.MetaDescription = metadataPart.MetaDescription;
                model.MetaKeywords = metadataPart.MetaKeywords;
                model.OpenGraphTitle = metadataPart.OpenGraphTitle;
                model.OpenGraphDescription = metadataPart.OpenGraphDescription;
                model.OpenGraphImage = metadataPart.OpenGraphImage;
                model.TwitterCard = metadataPart.TwitterCard;
                model.OpenGraphImageAlt = metadataPart.OpenGraphImageAlt;
                model.MetadataPart = metadataPart;
                model.Settings = settings;

            }).Location("Parts#Metadata:10");
        }

        public override async Task<IDisplayResult> UpdateAsync(MetadataPart model, IUpdateModel updater)
        {
            var settings = GetSettings(model);

            await updater.TryUpdateModelAsync(model, Prefix);

            return Edit(model);
        }

        public SocialMetadataPartSettings GetSettings(MetadataPart metadataPart)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(metadataPart.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, nameof(MetadataPart), StringComparison.Ordinal));
            return contentTypePartDefinition.Settings.ToObject<SocialMetadataPartSettings>();
        }

        private async Task BuildViewModelAsync(MetadataPartViewModel viewModel, MetadataPart part)
        {
            var settings = GetSettings(part);
            var siteSettings = await _siteService.GetSiteSettingsAsync();

            // If blank get title from Content items display text
            viewModel.MetaTitle = GenerateText(part.MetaTitle, part.ContentItem.DisplayText);
            viewModel.MetaDescription = part.MetaDescription;
            viewModel.MetaKeywords = part.MetaKeywords;

            // If blank get title from MetaTitle
            viewModel.OpenGraphTitle = GenerateText(part.OpenGraphTitle, viewModel.MetaTitle);

            // If blank get description from MetaDescription
            viewModel.OpenGraphDescription = GenerateText(part.OpenGraphDescription, viewModel.MetaDescription);
            viewModel.OpenGraphImage = part.OpenGraphImage;
            viewModel.OpenGraphVideo = part.OpenGraphVideo;

            // If blank default to summary card
            viewModel.TwitterCard = GenerateText(part.TwitterCard, "summary");

            // If blank get image alternate text from OpenGraphTitle
            viewModel.OpenGraphImageAlt = GenerateText(part.OpenGraphImageAlt, viewModel.OpenGraphTitle);

            // Get site name from site settings
            viewModel.OpenGraphSite_name = siteSettings.SiteName;
            viewModel.OpenGraphType = GenerateText(part.OpenGraphType, "website");

            // TODO not quite sure how to get the full url of the page
            viewModel.OpenGraphUrl = siteSettings.BaseUrl;
            viewModel.MetadataPart = part;
            viewModel.Settings = settings;

        }

        private string GenerateText(string text, string alternateText)
        {
            return String.IsNullOrWhiteSpace(text) ? alternateText : text;
        }
    }
}
