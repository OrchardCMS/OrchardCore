using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.AdminTrees.Services;
using OrchardCore.Navigation;
using System.Linq;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata.Models;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using System.Threading.Tasks;

namespace OrchardCore.Contents.AdminNodes
{
    public class ContentTypesAdminNodeNavigationBuilder : IAdminNodeNavigationBuilder
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ILogger<ContentTypesAdminNodeNavigationBuilder> _logger;
        private readonly string _contentItemlistUrl;

        public ContentTypesAdminNodeNavigationBuilder(
            IContentDefinitionManager contentDefinitionManager,
            ShellSettings shellSettings,
            ILogger<ContentTypesAdminNodeNavigationBuilder> logger)
        {
            _contentDefinitionManager = contentDefinitionManager;

            var tenantPrefix = ('/' + (shellSettings.RequestUrlPrefix ?? string.Empty)).TrimEnd('/');
            _contentItemlistUrl = tenantPrefix + "/Admin/Contents/ContentItems/";


            _logger = logger;
        }

        public string Name => typeof(ContentTypesAdminNode).Name;


        public void BuildNavigation(MenuItem treeNode, NavigationBuilder builder, IEnumerable<IAdminNodeNavigationBuilder> treeNodeBuilders)
        {
            var tn = treeNode as ContentTypesAdminNode;

            if ((tn == null) || (!tn.Enabled))
            {
                return;
            }

            // Add ContentTypes specific children
            var contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions().OrderBy(d => d.Name);
            var typesToShow = GetContentTypes(tn);
            foreach (var ctd in typesToShow)
            {
                builder.Add(new LocalizedString(ctd.DisplayName, ctd.DisplayName), cTypeMenu =>
                {
                    cTypeMenu.Url(_contentItemlistUrl + ctd.Name);
                    tn.CustomClasses.ToList().ForEach(x => cTypeMenu.AddClass(x));
                });
            }

            // Add external children
            var itemBuilder = builder as NavigationItemBuilder;
            if (itemBuilder != null)
            {
                foreach (var childTreeNode in tn.Items)
                {
                    try
                    {
                        var treeBuilder = treeNodeBuilders.Where(x => x.Name == childTreeNode.GetType().Name).FirstOrDefault();
                        treeBuilder.BuildNavigation(childTreeNode, itemBuilder, treeNodeBuilders);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "An exception occurred while building the '{MenuItem}' child Menu Item.", childTreeNode.GetType().Name);
                    }
                }
            }

            return;
        }


        private IEnumerable<ContentTypeDefinition> GetContentTypes(ContentTypesAdminNode tn)
        {
            var typesToShow = _contentDefinitionManager.ListTypeDefinitions().
                Where(ctd => ctd.Settings.ToObject<ContentTypeSettings>().Listable);

            if (tn.ShowAll == false)
            {
                typesToShow = typesToShow.Where(ctd => tn.ContentTypes.ToList<string>().Contains(ctd.Name));
            }

            return typesToShow.OrderBy(t => t.Name);
        }
    }

}
