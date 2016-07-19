using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Orchard.ResourceManagement
{
    public class ResourceRequiredContext
    {
        public ResourceDefinition Resource { get; set; }
        public RequireSettings Settings { get; set; }

        public string GetResourceUrl(RequireSettings baseSettings, string appPath)
        {
            return Resource.ResolveUrl(baseSettings == null ? Settings : baseSettings.Combine(Settings), appPath);
        }

        public TagBuilder GetTagBuilder(RequireSettings baseSettings, string appPath)
        {
            var tagBuilder = new TagBuilder(Resource.TagName);
            tagBuilder.MergeAttributes(Resource.TagBuilder.Attributes);
            if (!String.IsNullOrEmpty(Resource.FilePathAttributeName))
            {
                var resolvedUrl = GetResourceUrl(baseSettings, appPath);
                if (!String.IsNullOrEmpty(resolvedUrl))
                {
                    tagBuilder.MergeAttribute(Resource.FilePathAttributeName, resolvedUrl, true);
                }
            }
            return tagBuilder;
        }
    }
}
