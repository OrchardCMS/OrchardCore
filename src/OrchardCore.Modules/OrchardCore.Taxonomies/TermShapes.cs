using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Utilities;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Taxonomies.Models;
using OrchardCore.Taxonomies.ViewModels;

namespace OrchardCore.Taxonomies
{
    public class TermShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            // Add standard alternates to a TermPart because it is rendered by a content display driver not a part display driver.
            builder.Describe("TermPart")
                .OnDisplaying(context =>
                {
                    var viewModel = context.Shape as TermPartViewModel;

                    var contentType = viewModel?.ContentItem?.ContentType;
                    var displayTypes = new[] { "", "_" + context.Shape.Metadata.DisplayType };

                    // [ShapeType]_[DisplayType], e.g. TermPart.Summary, TermPart.Detail.
                    context.Shape.Metadata.Alternates.Add($"TermPart_{context.Shape.Metadata.DisplayType}");

                    foreach (var displayType in displayTypes)
                    {
                        // [ContentType]_[DisplayType]__[PartType], e.g. Category-TermPart, Category-TermPart.Detail.
                        context.Shape.Metadata.Alternates.Add($"{contentType}{displayType}__TermPart");
                    }
                });

            builder.Describe("Term")
                .OnProcessing(async context =>
                {
                    var termShape = context.Shape;
                    var identifier = termShape.GetProperty<string>("TaxonomyContentItemId") ?? termShape.GetProperty<string>("Alias");

                    if (String.IsNullOrEmpty(identifier))
                    {
                        return;
                    }

                    termShape.Classes.Add("term");

                    // Term population is executed when processing the shape so that its value
                    // can be cached. IShapeDisplayEvents is called before the ShapeDescriptor
                    // events and thus this code can be cached.

                    var shapeFactory = context.ServiceProvider.GetRequiredService<IShapeFactory>();
                    var contentManager = context.ServiceProvider.GetRequiredService<IContentManager>();
                    var handleManager = context.ServiceProvider.GetRequiredService<IContentHandleManager>();

                    var taxonomyContentItemId = termShape.TryGetProperty("Alias", out object alias) && alias != null
                        ? await handleManager.GetContentItemIdAsync(alias.ToString())
                        : termShape.Properties["TaxonomyContentItemId"].ToString();

                    if (taxonomyContentItemId == null)
                    {
                        return;
                    }

                    var taxonomyContentItem = await contentManager.GetAsync(taxonomyContentItemId);

                    if (taxonomyContentItem == null)
                    {
                        return;
                    }

                    termShape.Properties["TaxonomyContentItem"] = taxonomyContentItem;
                    termShape.Properties["TaxonomyName"] = taxonomyContentItem.DisplayText;

                    var taxonomyPart = taxonomyContentItem.As<TaxonomyPart>();
                    if (taxonomyPart == null)
                    {
                        return;
                    }

                    // When a TermContentItemId is provided render the term and its child terms.
                    var level = 0;
                    List<ContentItem> termItems = null;
                    var termContentItemId = termShape.GetProperty<string>("TermContentItemId");
                    if (!String.IsNullOrEmpty(termContentItemId))
                    {
                        level = FindTerm(taxonomyContentItem.Content.TaxonomyPart.Terms as JArray, termContentItemId, level, out var termContentItem);

                        if (termContentItem == null)
                        {
                            return;
                        }

                        termItems = new List<ContentItem>
                        {
                            termContentItem
                        };
                    }
                    else
                    {
                        termItems = taxonomyPart.Terms;
                    }

                    if (termItems == null)
                    {
                        return;
                    }

                    var differentiator = FormatName(termShape.GetProperty<string>("TaxonomyName"));

                    if (!String.IsNullOrEmpty(differentiator))
                    {
                        // Term__[Differentiator] e.g. Term-Categories, Term-Tags.
                        termShape.Metadata.Alternates.Add("Term__" + differentiator);
                        termShape.Metadata.Differentiator = differentiator;
                        termShape.Classes.Add(("term-" + differentiator).HtmlClassify());
                    }

                    termShape.Classes.Add(("term-" + taxonomyPart.TermContentType).HtmlClassify());

                    var encodedContentType = taxonomyPart.TermContentType.EncodeAlternateElement();
                    // Term__[ContentType] e.g. Term-Category, Term-Tag.
                    termShape.Metadata.Alternates.Add("Term__" + encodedContentType);

                    // The first level of term item shapes is created.
                    // Each other level is created when the term item is displayed.

                    foreach (var termContentItem in termItems)
                    {
                        ContentItem[] childTerms = null;
                        if (termContentItem.Content.Terms is JArray termsArray)
                        {
                            childTerms = termsArray.ToObject<ContentItem[]>();
                        }

                        var shape = await shapeFactory.CreateAsync("TermItem", Arguments.From(new
                        {
                            Level = level,
                            Term = termShape,
                            TermContentItem = termContentItem,
                            Terms = childTerms ?? Array.Empty<ContentItem>(),
                            TaxonomyContentItem = taxonomyContentItem
                        }));

                        shape.Metadata.Differentiator = differentiator;

                        // Don't use Items.Add() or the collection won't be sorted.
                        await termShape.AddAsync(shape);
                    }
                });

            builder.Describe("TermItem")
                .OnDisplaying(async context =>
                {
                    var termItem = context.Shape;
                    var termShape = termItem.GetProperty<IShape>("Term");
                    var level = termItem.GetProperty<int>("Level");
                    var taxonomyContentItem = termItem.GetProperty<ContentItem>("TaxonomyContentItem");
                    var taxonomyPart = taxonomyContentItem.As<TaxonomyPart>();
                    var differentiator = termItem.Metadata.Differentiator;

                    var shapeFactory = context.ServiceProvider.GetRequiredService<IShapeFactory>();

                    if (termItem.GetProperty<ContentItem[]>("Terms") != null)
                    {
                        foreach (var termContentItem in termItem.GetProperty<ContentItem[]>("Terms"))
                        {
                            ContentItem[] childTerms = null;
                            if (termContentItem.Content.Terms is JArray termsArray)
                            {
                                childTerms = termsArray.ToObject<ContentItem[]>();
                            }
                            var shape = await shapeFactory.CreateAsync("TermItem", Arguments.From(new
                            {
                                Level = level + 1,
                                TaxonomyContentItem = taxonomyContentItem,
                                TermContentItem = termContentItem,
                                Term = termShape,
                                Terms = childTerms ?? Array.Empty<ContentItem>()
                            }));

                            shape.Metadata.Differentiator = differentiator;

                            // Don't use Items.Add() or the collection won't be sorted.
                            await termItem.AddAsync(shape);
                        }
                    }

                    var encodedContentType = taxonomyPart.TermContentType.EncodeAlternateElement();

                    // TermItem__level__[level] e.g. TermItem-level-2.
                    termItem.Metadata.Alternates.Add("TermItem__level__" + level);

                    // TermItem__[ContentType] e.g. TermItem-Category
                    // TermItem__[ContentType]__level__[level] e.g. TermItem-Category-level-2.
                    termItem.Metadata.Alternates.Add("TermItem__" + encodedContentType);
                    termItem.Metadata.Alternates.Add("TermItem__" + encodedContentType + "__level__" + level);

                    if (!String.IsNullOrEmpty(differentiator))
                    {
                        // TermItem__[Differentiator] e.g. TermItem-Categories, TermItem-Travel.
                        // TermItem__[Differentiator]__level__[level] e.g. TermItem-Categories-level-2.
                        termItem.Metadata.Alternates.Add("TermItem__" + differentiator);
                        termItem.Metadata.Alternates.Add("TermItem__" + differentiator + "__level__" + level);

                        // TermItem__[Differentiator]__[ContentType] e.g. TermItem-Categories-Category.
                        // TermItem__[Differentiator]__[ContentType]__level__[level] e.g. TermItem-Categories-Category-level-2.
                        termItem.Metadata.Alternates.Add("TermItem__" + differentiator + "__" + encodedContentType);
                        termItem.Metadata.Alternates.Add("TermItem__" + differentiator + "__" + encodedContentType + "__level__" + level);
                    }
                });

            builder.Describe("TermContentItem")
                .OnDisplaying(displaying =>
                {
                    var termItem = displaying.Shape;
                    var level = termItem.GetProperty<int>("Level");
                    var differentiator = termItem.Metadata.Differentiator;

                    var termContentItem = termItem.GetProperty<ContentItem>("TermContentItem");

                    var encodedContentType = termContentItem.ContentItem.ContentType.EncodeAlternateElement();

                    termItem.Metadata.Alternates.Add("TermContentItem__level__" + level);

                    // TermContentItem__[ContentType] e.g. TermContentItem-Category.
                    // TermContentItem__[ContentType]__level__[level] e.g. TermContentItem-Category-level-2.
                    termItem.Metadata.Alternates.Add("TermContentItem__" + encodedContentType);
                    termItem.Metadata.Alternates.Add("TermContentItem__" + encodedContentType + "__level__" + level);

                    if (!String.IsNullOrEmpty(differentiator))
                    {
                        // TermContentItem__[Differentiator] e.g. TermContentItem-Categories.
                        termItem.Metadata.Alternates.Add("TermContentItem__" + differentiator);
                        // TermContentItem__[Differentiator]__level__[level] e.g. TermContentItem-Categories-level-2.
                        termItem.Metadata.Alternates.Add("TermContentItem__" + differentiator + "__level__" + level);

                        // TermContentItem__[Differentiator]__[ContentType] e.g. TermContentItem-Categories-Category.
                        // TermContentItem__[Differentiator]__[ContentType] e.g. TermContentItem-Categories-Category-level-2.
                        termItem.Metadata.Alternates.Add("TermContentItem__" + differentiator + "__" + encodedContentType);
                        termItem.Metadata.Alternates.Add("TermContentItem__" + differentiator + "__" + encodedContentType + "__level__" + level);
                    }
                });
        }

        private int FindTerm(JArray termsArray, string termContentItemId, int level, out ContentItem contentItem)
        {
            foreach (var term in termsArray.Cast<JObject>())
            {
                var contentItemId = term.GetValue("ContentItemId").ToString();

                if (contentItemId == termContentItemId)
                {
                    contentItem = term.ToObject<ContentItem>();
                    return level;
                }

                if (term.GetValue("Terms") is JArray children)
                {
                    level += 1;
                    level = FindTerm(children, termContentItemId, level, out var foundContentItem);

                    if (foundContentItem != null)
                    {
                        contentItem = foundContentItem;
                        return level;
                    }
                }
            }
            contentItem = null;

            return level;
        }

        /// <summary>
        /// Converts "foo-ba r" to "FooBaR".
        /// </summary>
        private static string FormatName(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                return null;
            }

            name = name.Trim();
            var nextIsUpper = true;
            var result = new StringBuilder(name.Length);
            for (var i = 0; i < name.Length; i++)
            {
                var c = name[i];

                if (c == '-' || Char.IsWhiteSpace(c))
                {
                    nextIsUpper = true;
                    continue;
                }

                if (nextIsUpper)
                {
                    result.Append(c.ToString().ToUpper());
                }
                else
                {
                    result.Append(c);
                }

                nextIsUpper = false;
            }

            return result.ToString();
        }
    }
}
