using System;
using System.Collections.Generic;

namespace Orchard.ResourceManagement
{
    public interface IResourceManager
    {
        IEnumerable<RequireSettings> GetRequiredResources(string type);
        IList<ResourceRequiredContext> BuildRequiredResources(string resourceType);
        IList<LinkEntry> GetRegisteredLinks();
        IList<MetaEntry> GetRegisteredMetas();
        IList<String> GetRegisteredHeadScripts();
        IList<String> GetRegisteredFootScripts();
        IEnumerable<IResourceManifest> ResourceProviders { get; }
        ResourceManifest DynamicResources { get; }
        ResourceDefinition FindResource(RequireSettings settings);
        void NotRequired(string resourceType, string resourceName);
        RequireSettings Include(string resourceType, string resourcePath, string resourceDebugPath);
        RequireSettings Include(string resourceType, string resourcePath, string resourceDebugPath, string relativeFromPath);
        RequireSettings Require(string resourceType, string resourceName);
        void RegisterHeadScript(string script);
        void RegisterFootScript(string script);
        void RegisterLink(LinkEntry link);
        void SetMeta(MetaEntry meta);
        void AppendMeta(MetaEntry meta, string contentSeparator);
    }
}
