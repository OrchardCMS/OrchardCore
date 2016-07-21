using System;
using System.Collections.Generic;

namespace Orchard.ResourceManagement
{
    public interface IResourceManager
    {
        IEnumerable<ResourceRequiredContext> GetRequiredResources(string resourceType);
        IEnumerable<LinkEntry> GetRegisteredLinks();
        IEnumerable<MetaEntry> GetRegisteredMetas();
        IEnumerable<string> GetRegisteredHeadScripts();
        IEnumerable<string> GetRegisteredFootScripts();

        /// <summary>
        /// Returns an inline manifest.
        /// </summary>
        ResourceManifest InlineManifest { get; }
        ResourceDefinition FindResource(RequireSettings settings);
        void NotRequired(string resourceType, string resourceName);
        RequireSettings Include(string resourceType, string resourcePath, string resourceDebugPath);

        /// <summary>
        /// Registers a custom url.
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="resourcePath"></param>
        /// <param name="resourceDebugPath"></param>
        /// <param name="relativeFromPath"></param>
        /// <returns></returns>
        RequireSettings RegisterUrl(string resourceType, string resourcePath, string resourceDebugPath, string relativeFromPath);

        /// <summary>
        /// Registers a named resource.
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        RequireSettings RegisterResource(string resourceType, string resourceName);

        /// <summary>
        /// Registers a custom script tag on at the head.
        /// </summary>
        /// <param name="script"></param>
        void RegisterHeadScript(string script);

        /// <summary>
        /// Registers a custom script tag on at the foot.
        /// </summary>
        /// <param name="script"></param>
        void RegisterFootScript(string script);

        /// <summary>
        /// Registers a link tag.
        /// </summary>
        /// <param name="link"></param>
        void RegisterLink(LinkEntry link);

        /// <summary>
        /// Registers a meta tag.
        /// </summary>
        /// <param name="meta"></param>
        void RegisterMeta(MetaEntry meta);

        /// <summary>
        /// Appends a value to the current content of a meta tag, separated by a custom separator.
        /// </summary>
        void AppendMeta(MetaEntry meta, string contentSeparator);
    }
}
