using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Orchard.ResourceManagement
{
    public class ResourceRequiredContext
    {
        public ResourceDefinition Resource { get; set; }
        public RequireSettings Settings { get; set; }

        public TagBuilder GetTagBuilder(RequireSettings baseSettings, string appPath)
        {
            return Resource.GetTagBuilder(baseSettings == null ? Settings : baseSettings.Combine(Settings), appPath);
        }
    }
}
