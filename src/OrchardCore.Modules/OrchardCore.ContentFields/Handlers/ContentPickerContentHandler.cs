using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Liquid;
using OrchardCore.Localization;

namespace OrchardCore.ContentFields.Handlers
{
    public class ContentPickerContentHandler : ContentHandlerBase
    {
        private readonly ILiquidTemplateManager _templateManager;

        public ContentPickerContentHandler(ILiquidTemplateManager templateManager)
        {
            _templateManager = templateManager;
        }

        public async override Task GetContentItemAspectAsync(ContentItemAspectContext context)
        {
            await context.ForAsync<ContentPickerAspect>(async aspect =>
            {
                var settings = aspect.ContentPickerFieldSettings as ContentPickerFieldSettings;

                if (settings != null) {
                    aspect.Title = await GetContentPickerItemExtendedDescription(context.ContentItem, aspect.Culture, settings.TitlePattern, context.ContentItem.ToString());
                    aspect.Description = await GetContentPickerItemExtendedDescription(context.ContentItem, aspect.Culture, settings.DescriptionPattern, string.Empty);
                }
            });
        }

        private async Task<string> GetContentPickerItemExtendedValue(ContentItem contentItem, CultureInfo culture, string pattern, string defaultValue)
        {
            var result = defaultValue;
            if (!string.IsNullOrEmpty(pattern))
            {
                
                using (CultureScope.Create(culture))
                {
                    result = await _templateManager.RenderStringAsync(pattern, NullEncoder.Default, contentItem,
                        new Dictionary<string, FluidValue>() { [nameof(ContentItem)] = new ObjectValue(contentItem) });
                }
            }

            return result;
        }
    }
}
