using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.Zones
{
    public class ZoneShapes : IShapeAttributeProvider
    {
        [Shape]
        public async Task<IHtmlContent> Zone(dynamic DisplayAsync, dynamic Shape)
        {
            var htmlContentBuilder = new HtmlContentBuilder();
            foreach (var item in Shape)
            {
                htmlContentBuilder.AppendHtml(await DisplayAsync(item));
            }

            return htmlContentBuilder;
        }

        [Shape]
        public async Task<IHtmlContent> ContentZone(dynamic DisplayAsync, dynamic Shape, dynamic New, DisplayContext DisplayContext)
        {
            var htmlContentBuilder = new HtmlContentBuilder();

            var shapes = ((IEnumerable<dynamic>)Shape);

            var groupings = shapes.GroupBy(x =>
            {
                // By convention all placement delimiters default to the name 'Content' when not specified during placement.
                var key = (string)x.Metadata.Tab;
                if (String.IsNullOrEmpty(key))
                {
                    key = "Content";
                }

                // Remove any positioning modifier.
                var modifierIndex = key.IndexOf(';');
                if (modifierIndex != -1)
                {
                    key = key.Substring(0, modifierIndex);
                }

                return key;
            }).ToList();

            // Process tabs first, then Cards, then Columns.
            if (groupings.Count > 1)
            {
                 var orderedGroupings = groupings.OrderBy(grouping => {
                    var firstGroupWithModifier = grouping.FirstOrDefault(group => {
                        var key = (string)group.Metadata.Tab;
                        if (!String.IsNullOrEmpty(key))
                        {
                            var modifierIndex = key.IndexOf(';');
                            if (modifierIndex != -1)
                            {
                                return true;
                            }
                        }
                        return false;
                    });

                    if (firstGroupWithModifier != null)
                    {
                        var key = (string)firstGroupWithModifier.Metadata.Tab;
                        var modifierIndex = key.IndexOf(';');
                        return new PositionalGrouping(key.Substring(modifierIndex));
                    }

                    return new PositionalGrouping(null);
                }, FlatPositionComparer.Instance).ToList();

                dynamic tabContainer = await New.TabContainer(ContentItem: Shape.ContentItem, Tabs: orderedGroupings);

                htmlContentBuilder.AppendHtml(await DisplayAsync(tabContainer));
            }
            else
            {
                // This determins whether to show a card, or fall back to columns
                var cardGrouping = await New.CardGrouping(Grouping: groupings[0], ContentItem: Shape.ContentItem);

                htmlContentBuilder.AppendHtml(await DisplayAsync(cardGrouping));
            }

            return htmlContentBuilder;
        }

        [Shape]
        public async Task<IHtmlContent> CardGrouping(dynamic DisplayAsync, dynamic Shape, dynamic New)
        {
            var htmlContentBuilder = new HtmlContentBuilder();
            IGrouping<string, dynamic> grouping = Shape.Grouping;

            var groupings = grouping.GroupBy(x =>
            {
                // By convention all placement delimiters default to the name 'Content' when not specified during placement.
                var key = (string)x.Metadata.Card;
                if (String.IsNullOrEmpty(key))
                {
                    key = "Content";
                }

                // Remove positional modifier.
                var modifierIndex = key.IndexOf(';');
                if (modifierIndex != -1)
                {
                    key = key.Substring(0, modifierIndex);
                }

                return key;
            }).ToList();

            if (groupings.Count > 1)
            {
                 var orderedGroupings = groupings.OrderBy(grouping => {
                    var firstGroupWithModifier = grouping.FirstOrDefault(group => {
                        var key = (string)group.Metadata.Card;
                        if (!String.IsNullOrEmpty(key))
                        {
                            var modifierIndex = key.IndexOf(';');
                            if (modifierIndex != -1)
                            {
                                return true;
                            }
                        }
                        return false;
                    });

                    if (firstGroupWithModifier != null)
                    {
                        var key = (string)firstGroupWithModifier.Metadata.Card;
                        var modifierIndex = key.IndexOf(';');
                        return new PositionalGrouping(key.Substring(modifierIndex));
                    }

                    return new PositionalGrouping(null);
                }, FlatPositionComparer.Instance).ToList();

                dynamic container = await New.CardContainer(ContentItem: Shape.ContentItem, Cards: orderedGroupings);
                htmlContentBuilder.AppendHtml(await DisplayAsync(container));
            }
            else
            {
                // See if there are columns.
                var groupingShape = await New.ColumnGrouping(Grouping: grouping, ContentItem: Shape.ContentItem);
                htmlContentBuilder.AppendHtml(await DisplayAsync(groupingShape));
            }

            return htmlContentBuilder;
        }

        [Shape]
        public async Task<IHtmlContent> ColumnGrouping(dynamic DisplayAsync, dynamic Shape, dynamic New)
        {
            var htmlContentBuilder = new HtmlContentBuilder();
            IGrouping<string, dynamic> grouping = Shape.Grouping;

            var groupings = grouping.GroupBy(x =>
            {
                // By convention all placement delimiters default to the name 'Content' when not specified during placement.
                var key = (string)x.Metadata.Column;
                if (String.IsNullOrEmpty(key))
                {
                    key = "Content";
                }

                // Remove column modifier.
                var modifierIndex = key.IndexOf('-');
                if (modifierIndex != -1)
                {
                    key = key.Substring(0, modifierIndex);
                }

                // Remove positional modifier.
                modifierIndex = key.IndexOf(';');
                if (modifierIndex != -1)
                {
                    key = key.Substring(0, modifierIndex);
                }

                return key;
            }).ToList();

            if (groupings.Count > 1)
            {
                 var orderedGroupings = groupings.OrderBy(grouping => {
                    var firstGroupWithModifier = grouping.FirstOrDefault(group => {
                        var key = (string)group.Metadata.Column;
                        if (!String.IsNullOrEmpty(key))
                        {
                            var modifierIndex = key.IndexOf(';');
                            if (modifierIndex != -1)
                            {
                                return true;
                            }
                        }
                        return false;
                    });

                    if (firstGroupWithModifier != null)
                    {
                        var key = (string)firstGroupWithModifier.Metadata.Column;
                        // Here we have to remove the - section
                        var columnModifierIndex = key.IndexOf('-');
                        if (columnModifierIndex != -1)
                        {
                            var positionModifierIndex = key.IndexOf(';');
                            // Column-9;56
                            if (positionModifierIndex > columnModifierIndex)
                            {
                                return new PositionalGrouping(key.Substring(positionModifierIndex));
                            }
                            else // Column;56-9 // positionmodifierinex here = 6. columnmod = 9
                            {
                                var length = columnModifierIndex - positionModifierIndex;
                                return new PositionalGrouping(key.Substring(positionModifierIndex, length));
                            }
                        }
                        else
                        {
                            var positionModifierIndex = key.IndexOf(';');
                            return new PositionalGrouping(key.Substring(positionModifierIndex));
                        }
                    }

                    return new PositionalGrouping(null);
                }, FlatPositionComparer.Instance).ToList();

                dynamic container = await New.ColumnContainer(ContentItem: Shape.ContentItem, Columns: orderedGroupings);
                htmlContentBuilder.AppendHtml(await DisplayAsync(container));
            }
            else
            {
                foreach (var item in grouping)
                {
                    htmlContentBuilder.AppendHtml(await DisplayAsync(item));
                }
            }

            return htmlContentBuilder;
        }
    }

    public class PositionalGrouping : IPositioned
    {
        public PositionalGrouping(string key)
        {
            if (!String.IsNullOrEmpty(key))
            {
                var modifierIndex = key.IndexOf(';');
                if (modifierIndex != -1)
                {
                    Position = key.Substring(modifierIndex);
                }
            }
        }
        public string Position { get; set; }
    }
}
