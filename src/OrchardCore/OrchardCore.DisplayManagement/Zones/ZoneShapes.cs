using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
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

            // Process Tabs first, then Cards, then Columns.
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

                dynamic container = await New.TabContainer(ContentItem: Shape.ContentItem, Grouping: orderedGroupings);
                foreach(var orderedGrouping in orderedGroupings)
                {
                    var groupingShape = await New.Tab(Grouping: orderedGrouping, ContentItem: Shape.ContentItem);
                    foreach(var item in orderedGrouping)
                    {
                        groupingShape.Add(item);
                    }
                    container.Add(groupingShape);
                }

                htmlContentBuilder.AppendHtml(await DisplayAsync(container));
            }
            else
            {
                // Evaluate for cards.
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

                    return new PositionalGrouping();
                }, FlatPositionComparer.Instance).ToList();

                dynamic container = await New.CardContainer(ContentItem: Shape.ContentItem);
                foreach(var orderedGrouping in orderedGroupings)
                {
                    var groupingShape = await New.Card(Grouping: orderedGrouping, ContentItem: Shape.ContentItem);
                    foreach(var item in orderedGrouping)
                    {
                        groupingShape.Add(item);
                    }
                    container.Add(groupingShape);
                }

                htmlContentBuilder.AppendHtml(await DisplayAsync(container));
            }
            else
            {
                // Evaluate for columns.
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

            var groupings = grouping.GroupBy(x => {
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
                var positionModifiers = GetColumnPositions(groupings);

                var orderedGroupings = groupings.OrderBy(grouping => {
                    if (positionModifiers.TryGetValue(grouping.Key, out var position))
                    {
                        return new PositionalGrouping { Position = position };
                    }
                    else
                    {
                        return new PositionalGrouping();
                    }
                }, FlatPositionComparer.Instance).ToList();

                var columnModifiers = GetColumnModifiers(orderedGroupings);

                dynamic container = await New.ColumnContainer(ContentItem: Shape.ContentItem);
                foreach(var orderedGrouping in orderedGroupings)
                {
                    var groupingShape = await New.Column(Grouping: orderedGrouping, ContentItem: Shape.ContentItem);

                    groupingShape.Classes.Add("column-" + orderedGrouping.Key.HtmlClassify());

                    var columnClass = "col";
                    if (columnModifiers.TryGetValue(orderedGrouping.Key, out var columnModifier))
                    {
                        columnClass = "col-" + columnModifier;
                    }

                    groupingShape.Classes.Add(columnClass);

                    foreach(var item in orderedGrouping)
                    {
                        groupingShape.Add(item);
                    }
                    container.Add(groupingShape);
                }

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
        private static Dictionary<string, string> GetColumnPositions(IList<IGrouping<string, dynamic>> groupings)
        {
            var positionModifiers = new Dictionary<string, string>();
            foreach(var grouping in groupings)
            {
                var firstGroupWithModifier = FirstGroupingWithModifierOrDefault(grouping, ';');
                if (firstGroupWithModifier != null)
                {
                    var key = (string)firstGroupWithModifier.Metadata.Column;
                    var columnModifierIndex = key.IndexOf('-');
                    if (columnModifierIndex != -1)
                    {
                        var positionModifierIndex = key.IndexOf(';');
                        // Column-9;56
                        if (positionModifierIndex > columnModifierIndex)
                        {
                            positionModifiers.Add(key.Substring(0, columnModifierIndex), key.Substring(positionModifierIndex + 1));
                        }
                        else // Column;56-9
                        {
                            var length = columnModifierIndex - positionModifierIndex;
                            positionModifiers.Add(key.Substring(0, positionModifierIndex), key.Substring(positionModifierIndex + 1, length - 1));
                        }
                    }
                    else
                    {
                        var positionModifierIndex = key.IndexOf(';');
                        positionModifiers.Add(key.Substring(0, positionModifierIndex), key.Substring(positionModifierIndex + 1));
                    }
                }
            }

            return positionModifiers;
        }

        private static Dictionary<string, string> GetColumnModifiers(IList<IGrouping<string, dynamic>> groupings)
        {
            var columnModifiers = new Dictionary<string, string>();
            foreach(var grouping in groupings)
            {
                var firstGroupWithModifier = FirstGroupingWithModifierOrDefault(grouping, '-');
                if (firstGroupWithModifier != null)
                {
                    var key = (string)firstGroupWithModifier.Metadata.Column;
                    var posModifierIndex = key.IndexOf(';');
                    if (posModifierIndex != -1)
                    {
                        var colModifierIndex = key.IndexOf('-');
                        // Column;5.1-9
                        if (colModifierIndex > posModifierIndex)
                        {
                            columnModifiers.Add(key.Substring(0, posModifierIndex), key.Substring(colModifierIndex + 1));
                        }
                        else // Column-9;5.1
                        {
                            var length = posModifierIndex - colModifierIndex;
                            columnModifiers.Add(key.Substring(0, colModifierIndex), key.Substring(colModifierIndex + 1, length - 1));
                        }
                    }
                    else
                    {
                        var columnModifierIndex = key.IndexOf('-');
                        columnModifiers.Add(key.Substring(0, columnModifierIndex), key.Substring(columnModifierIndex + 1));
                    }
                }
            }

            return columnModifiers;
        }

        private static dynamic FirstGroupingWithModifierOrDefault(IGrouping<string, dynamic> grouping, char modifier)
        {
            var firstGroupWithModifier = grouping.FirstOrDefault(group =>
            {
                var key = (string)group.Metadata.Column;
                if (!String.IsNullOrEmpty(key))
                {
                    var modifierIndex = key.IndexOf(modifier);
                    if (modifierIndex != -1)
                    {
                        return true;
                    }
                }
                return false;
            });

            return firstGroupWithModifier;
        }
    }

    internal class PositionalGrouping : IPositioned
    {
        public PositionalGrouping()
        {
        }

        public PositionalGrouping(string key)
        {
            if (!String.IsNullOrEmpty(key))
            {
                var modifierIndex = key.IndexOf(';');
                if (modifierIndex != -1)
                {
                    Position = key.Substring(modifierIndex + 1);
                }
            }
        }
        public string Position { get; set; }
    }
}
