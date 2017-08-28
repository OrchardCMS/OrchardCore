using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.JsonApi.ContentDisplay;

namespace Orchard.JsonApi
{
    public class ApiContentManager : IApiContentManager
    {
        private readonly IEnumerable<IApiFieldDriver> _fieldDrivers;
        private readonly IEnumerable<IApiPartDriver> _partDrivers;
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITypeActivatorFactory<ContentPart> _contentPartFactory;

        public ApiContentManager(
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IApiFieldDriver> fieldDrivers,
            IEnumerable<IApiPartDriver> partDrivers,
            IContentManager contentManager,
            ITypeActivatorFactory<ContentPart> contentPartFactory,
            ILogger<ApiContentManager> logger)
        {
            _contentPartFactory = contentPartFactory;
            _contentDefinitionManager = contentDefinitionManager;
            _fieldDrivers = fieldDrivers;
            _partDrivers = partDrivers;
            _contentManager = contentManager;
            Logger = logger;
        }

        ILogger Logger { get; set; }

        public async Task<ApiItem> BuildAsync(ContentItem contentItem, IUrlHelper urlHelper, IUpdateModel updater)
        {
            if (contentItem == null)
            {
                throw new ArgumentNullException(nameof(contentItem));
            }

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            if (contentTypeDefinition == null)
            {
                return null;
            }

            var apiItem = new ApiItem(contentItem, urlHelper);

            var context = new BuildApiDisplayContext(
               apiItem,
               urlHelper,
               updater
            );

            //if (!apiItem.Latest)
            //{
            //    var draft = await _contentManager.GetAsync(contentItem.ContentItemId, VersionOptions.Draft);

            //    apiItem.AddLatest(draft);
            //}
            //else
            //{
            //    apiItem.AddLatest(contentItem);
            //}

            //if (!apiItem.Published)
            //{
            //    var published = await _contentManager.GetAsync(contentItem.ContentItemId, VersionOptions.Published);

            //    apiItem.AddPublished(published);
            //}
            //else
            //{
            //    apiItem.AddPublished(contentItem);
            //}

            foreach (var contentTypePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = contentTypePartDefinition.Name;
                var partTypeName = contentTypePartDefinition.PartDefinition.Name;
                var partActivator = _contentPartFactory.GetTypeActivator(partTypeName);
                var part = contentItem.Get(partActivator.Type, partName) as ContentPart;

                if (part != null)
                {
                    apiItem.AddPart(part);
                    foreach (var driver in _partDrivers)
                    {
                        try
                        {
                            if (driver.CanApply(part))
                            {
                                await driver.Apply(part, context);
                            }
                        }
                        catch (Exception ex)
                        {
                            InvokeExtensions.HandleException(ex, Logger, driver.GetType().Name, "BuildAsync");
                        }
                    }


                    foreach (var contentPartFieldDefinition in contentTypePartDefinition.PartDefinition.Fields)
                    {
                        foreach (var driver in _fieldDrivers)
                        {
                            try
                            {
                                if (driver.CanApply(contentPartFieldDefinition))
                                {
                                    await driver.Apply(contentPartFieldDefinition, context);
                                }
                            }
                            catch (Exception ex)
                            {
                                InvokeExtensions.HandleException(ex, Logger, driver.GetType().Name, "BuildAsync");
                            }
                        }
                    }
                }
            }

            return apiItem;
        }

        public Task<ApiItem> UpdateAsync(ContentItem content, IUpdateModel updater)
        {
            throw new NotImplementedException();
        }
    }

    public interface IApiFieldDriver {
        bool CanApply(ContentPartFieldDefinition field);
        Task Apply(ContentPartFieldDefinition field, BuildApiDisplayContext context);
    }

    public interface IApiPartDriver {
        bool CanApply(ContentPart part);
        Task Apply(ContentPart part, BuildApiDisplayContext context);
    }
}
