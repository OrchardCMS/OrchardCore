using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Liquid.Tags;
using OrchardCore.Liquid;

namespace OrchardCore.Contents.Liquid
{
    public class ContentAnchorTag : IAnchorTag
    {
        public int Order => -10;

        public bool Match(List<FilterArgument> argumentsList)
        {
            foreach (var argument in argumentsList)
            {
                switch (argument.Name)
                {
                    case "admin_for":
                    case "display_for":
                    case "edit_for":
                    case "remove_for":
                    case "create_for": return true;
                }
            }

            return false;
        }

        public async ValueTask<Completion> WriteToAsync(List<FilterArgument> argumentsList, IReadOnlyList<Statement> statements, TextWriter writer, TextEncoder encoder, LiquidTemplateContext context)
        {
            var services = context.Services;
            var viewContext = context.ViewContext;
            var urlHelperFactory = services.GetRequiredService<IUrlHelperFactory>();
            var contentManager = services.GetRequiredService<IContentManager>();

            ContentItem adminFor = null;
            ContentItem displayFor = null;
            ContentItem editFor = null;
            ContentItem removeFor = null;
            ContentItem createFor = null;

            Dictionary<string, string> routeValues = null;
            Dictionary<string, string> customAttributes = new Dictionary<string, string>();

            foreach (var argument in argumentsList)
            {
                switch (argument.Name)
                {
                    case "admin_for": adminFor = (await argument.Expression.EvaluateAsync(context)).ToObjectValue() as ContentItem; break;
                    case "display_for": displayFor = (await argument.Expression.EvaluateAsync(context)).ToObjectValue() as ContentItem; break;
                    case "edit_for": editFor = (await argument.Expression.EvaluateAsync(context)).ToObjectValue() as ContentItem; break;
                    case "remove_for": removeFor = (await argument.Expression.EvaluateAsync(context)).ToObjectValue() as ContentItem; break;
                    case "create_for": createFor = (await argument.Expression.EvaluateAsync(context)).ToObjectValue() as ContentItem; break;

                    default:

                        if (argument.Name.StartsWith("route_", StringComparison.OrdinalIgnoreCase))
                        {
                            routeValues ??= new Dictionary<string, string>();
                            routeValues[argument.Name[6..]] = (await argument.Expression.EvaluateAsync(context)).ToStringValue();
                        }
                        else
                        {
                            customAttributes[argument.Name] = (await argument.Expression.EvaluateAsync(context)).ToStringValue();
                        }

                        break;
                }
            }

            ContentItem contentItem = null;

            var urlHelper = urlHelperFactory.GetUrlHelper(viewContext);

            if (displayFor != null)
            {
                contentItem = displayFor;
                var previewAspect = await contentManager.PopulateAspectAsync<PreviewAspect>(contentItem);

                if (!String.IsNullOrEmpty(previewAspect.PreviewUrl))
                {
                    var previewUrl = previewAspect.PreviewUrl;
                    if (!previewUrl.StartsWith("~/", StringComparison.OrdinalIgnoreCase))
                    {
                        if (previewUrl.StartsWith('/'))
                        {
                            previewUrl = '~' + previewUrl;
                        }
                        else
                        {
                            previewUrl = "~/" + previewUrl;
                        }
                    }

                    customAttributes["href"] = urlHelper.Content(previewUrl);
                }
                else
                {
                    var metadata = await contentManager.PopulateAspectAsync<ContentItemMetadata>(displayFor);

                    if (metadata.DisplayRouteValues != null)
                    {
                        if (routeValues != null)
                        {
                            foreach (var attribute in routeValues)
                            {
                                metadata.DisplayRouteValues.Add(attribute.Key, attribute.Value);
                            }
                        }

                        customAttributes["href"] = urlHelper.Action(metadata.DisplayRouteValues["action"].ToString(), metadata.DisplayRouteValues);
                    }
                }
            }
            else if (editFor != null)
            {
                var metadata = await PopulateAspectForContentItemMetadataAsync(editFor);

                if (metadata.EditorRouteValues != null)
                {
                    if (routeValues != null)
                    {
                        foreach (var attribute in routeValues)
                        {
                            metadata.EditorRouteValues.Add(attribute.Key, attribute.Value);
                        }
                    }

                    customAttributes["href"] = urlHelper.Action(metadata.EditorRouteValues["action"].ToString(), metadata.EditorRouteValues);
                }
            }
            else if (adminFor != null)
            {
                var metadata = await PopulateAspectForContentItemMetadataAsync(adminFor);

                if (metadata.AdminRouteValues != null)
                {
                    if (routeValues != null)
                    {
                        foreach (var attribute in routeValues)
                        {
                            metadata.AdminRouteValues.Add(attribute.Key, attribute.Value);
                        }
                    }

                    customAttributes["href"] = urlHelper.Action(metadata.AdminRouteValues["action"].ToString(), metadata.AdminRouteValues);
                }
            }
            else if (removeFor != null)
            {
                var metadata = await PopulateAspectForContentItemMetadataAsync(removeFor);

                if (metadata.RemoveRouteValues != null)
                {
                    if (routeValues != null)
                    {
                        foreach (var attribute in routeValues)
                        {
                            metadata.RemoveRouteValues.Add(attribute.Key, attribute.Value);
                        }
                    }

                    customAttributes["href"] = urlHelper.Action(metadata.RemoveRouteValues["action"].ToString(), metadata.RemoveRouteValues);
                }
            }
            else if (createFor != null)
            {
                var metadata = await PopulateAspectForContentItemMetadataAsync(createFor);

                if (metadata.CreateRouteValues != null)
                {
                    if (routeValues != null)
                    {
                        foreach (var attribute in routeValues)
                        {
                            metadata.CreateRouteValues.Add(attribute.Key, attribute.Value);
                        }
                    }

                    customAttributes["href"] = urlHelper.Action(metadata.CreateRouteValues["action"].ToString(), metadata.CreateRouteValues);
                }
            }

            if (customAttributes.Count == 0)
            {
                return Completion.Normal;
            }

            var tagBuilder = new TagBuilder("a");

            foreach (var attribute in customAttributes)
            {
                tagBuilder.Attributes[attribute.Key] = attribute.Value;
            }

            tagBuilder.RenderStartTag().WriteTo(writer, (HtmlEncoder)encoder);

            if (statements != null && statements.Count > 0)
            {
                var completion = await statements.RenderStatementsAsync(writer, encoder, context);

                if (completion != Completion.Normal)
                {
                    return completion;
                }
            }
            else if (!String.IsNullOrEmpty(contentItem.DisplayText))
            {
                writer.Write(encoder.Encode(contentItem.DisplayText));
            }
            else
            {
                var contentDefinitionManager = services.GetRequiredService<IContentDefinitionManager>();
                var typeDefinition = contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
                writer.Write(encoder.Encode(typeDefinition.ToString()));
            }

            tagBuilder.RenderEndTag().WriteTo(writer, (HtmlEncoder)encoder);

            return Completion.Normal;

            async Task<ContentItemMetadata> PopulateAspectForContentItemMetadataAsync(ContentItem item)
            {
                contentItem = item;

                return await contentManager.PopulateAspectAsync<ContentItemMetadata>(item);
            }
        }
    }
}
