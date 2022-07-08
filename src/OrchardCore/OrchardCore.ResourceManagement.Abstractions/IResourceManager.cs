using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.ResourceManagement
{
    public interface IResourceManager
    {
        /// <summary>
        /// Returns an inline manifest.
        /// </summary>
        ResourceManifest InlineManifest { get; }

        /// <summary>
        /// Returns the resource matching the specific settings.
        /// </summary>
        ResourceDefinition FindResource(RequireSettings settings);

        /// <summary>
        /// Removes a resource from the registrations.
        /// </summary>
        void NotRequired(string resourceType, string resourceName);

        /// <summary>
        /// Registers a specific resource url.
        /// </summary>
        RequireSettings RegisterUrl(string resourceType, string resourcePath, string resourceDebugPath);

        /// <summary>
        /// Registers a named resource.
        /// </summary>
        RequireSettings RegisterResource(string resourceType, string resourceName);

        /// <summary>
        /// Registers a custom script tag at the head.
        /// </summary>
        void RegisterHeadScript(IHtmlContent script);

        /// <summary>
        /// Registers a custom script tag at the foot.
        /// </summary>
        /// <param name="script"></param>
        void RegisterFootScript(IHtmlContent script);

        /// <summary>
        /// Registers a custom style tag at the head.
        /// </summary>
        /// <param name="style"></param>
        void RegisterStyle(IHtmlContent style);

        /// <summary>
        /// Registers a link tag.
        /// </summary>
        void RegisterLink(LinkEntry link);

        /// <summary>
        /// Registers a meta tag.
        /// </summary>
        void RegisterMeta(MetaEntry meta);

        /// <summary>
        /// Appends a value to the current content of a meta tag, separated by a custom separator.
        /// </summary>
        void AppendMeta(MetaEntry meta, string contentSeparator);

        /// <summary>
        /// Returns the required resources of the specified type.
        /// </summary>
        IEnumerable<ResourceRequiredContext> GetRequiredResources(string resourceType);

        /// <summary>
        /// Returns the registered link resources.
        /// </summary>
        IEnumerable<LinkEntry> GetRegisteredLinks();

        /// <summary>
        /// Returns the registered meta resources.
        /// </summary>
        IEnumerable<MetaEntry> GetRegisteredMetas();

        /// <summary>
        /// Returns the registered header script resources.
        /// </summary>
        IEnumerable<IHtmlContent> GetRegisteredHeadScripts();

        /// <summary>
        /// Returns the registered footer script resources.
        /// </summary>
        IEnumerable<IHtmlContent> GetRegisteredFootScripts();

        /// <summary>
        /// Returns the registered style resources.
        /// </summary>
        IEnumerable<IHtmlContent> GetRegisteredStyles();

        /// <summary>
        /// Renders the registered meta tags.
        /// </summary>
        void RenderMeta(TextWriter writer);

        /// <summary>
        /// Renders the registered header link tags.
        /// </summary>
        void RenderHeadLink(TextWriter writer);

        /// <summary>
        /// Renders the registered stylesheets.
        /// </summary>
        void RenderStylesheet(TextWriter writer);

        /// <summary>
        /// Renders the registered header script tags.
        /// </summary>
        void RenderHeadScript(TextWriter writer);

        /// <summary>
        /// Renders the registered footer script tags.
        /// </summary>
        void RenderFootScript(TextWriter writer);

        /// <summary>
        /// Renders the registered local script tags.
        /// </summary>
        void RenderLocalScript(RequireSettings settings, TextWriter writer);

        /// <summary>
        /// Renders the registered local style tags.
        /// </summary>
        void RenderLocalStyle(RequireSettings settings, TextWriter writer);
    }
}
