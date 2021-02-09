using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;


namespace Orchard.Rules.Drivers
{
    /// <summary>
    /// Saves references to content items which have been displayed during a request
    /// </summary>
    public class DisplayedContentTypeDriver : ContentDisplayDriver, IDisplayedContentItemDriver
    {
        private readonly HashSet<string> _contentTypes = new HashSet<string>();

        public override Task<IDisplayResult> DisplayAsync(ContentItem contentItem, BuildDisplayContext context)
        {
            // TODO stereotype;
            if (context.DisplayType == "Detail")
            {
                _contentTypes.Add(contentItem.ContentType);
            }

            return Task.FromResult<IDisplayResult>(null);
        }

        public bool IsDisplayed(string contentType)
        {
            return _contentTypes.Contains(contentType);
        }
    }

    public interface IDisplayedContentItemDriver
    {
        bool IsDisplayed(string contentType);
    }
}