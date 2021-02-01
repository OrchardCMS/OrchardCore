using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.ViewModels;
using OrchardCore.Modules;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.Zones
{
    [Feature(Application.DefaultFeatureId)]
    public class ZoneShapes : IShapeAttributeProvider
    {
        [Shape]
        public async Task<IHtmlContent> Zone(IDisplayHelper DisplayAsync, IEnumerable<object> Shape)
        {
            var htmlContentBuilder = new HtmlContentBuilder();
            foreach (var item in Shape)
            {
                htmlContentBuilder.AppendHtml(await DisplayAsync.ShapeExecuteAsync(item));
            }

            return htmlContentBuilder;
        }

        [Shape]
        public async Task<IHtmlContent> ContentZone(IDisplayHelper DisplayAsync, dynamic Shape, IShapeFactory ShapeFactory)
        {
            var htmlContentBuilder = new HtmlContentBuilder();

            var shapes = ((IEnumerable<dynamic>)Shape);

            // Evaluate shapes for grouping metadata.
            var isGrouped = shapes.Any(x => x is IShape s && 
                (!String.IsNullOrEmpty(s.Metadata.Tab) || 
                !String.IsNullOrEmpty(s.Metadata.Card) || 
                !String.IsNullOrEmpty(s.Metadata.Column)));

            // When there is no grouping metadata on any shapes just render the Zone.
            if (!isGrouped)
            {
                foreach (var item in shapes)
                {
                    htmlContentBuilder.AppendHtml(await DisplayAsync.ShapeExecuteAsync(item));
                }

                return htmlContentBuilder;
            }

            string identifier = Shape.Identifier ?? String.Empty;

            var groupings = shapes.GroupBy(x =>
            {
                var s = (IShape)x;
                // By convention all placement delimiters default to the name 'Content' when not specified during placement.
                var key = s.Metadata.Tab;
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
            }, x => (IShape)x).ToList();

            // Process Tabs first, then Cards, then Columns.
            if (groupings.Count > 1)
            {
                var orderedGroupings = groupings.OrderBy(grouping =>
                {
                    var firstGroupWithModifier = grouping.FirstOrDefault(group =>
                    {
                        var key = group.Metadata.Tab;
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
                        var key = firstGroupWithModifier.Metadata.Tab;
                        var modifierIndex = key.IndexOf(';');
                        return new PositionalGrouping(key.Substring(modifierIndex));
                    }

                    return new PositionalGrouping(null);
                }, FlatPositionComparer.Instance).ToList();

                var container = (GroupingsViewModel)await ShapeFactory.CreateAsync<GroupingsViewModel>("TabContainer", m =>
                {
                    m.Identifier = identifier;
                    m.Groupings = orderedGroupings;
                });

                foreach (var orderedGrouping in orderedGroupings)
                {
                    var groupingShape = (GroupingViewModel)await ShapeFactory.CreateAsync<GroupingViewModel>("Tab", m =>
                    {
                        m.Identifier = identifier;
                        m.Grouping = orderedGrouping;
                    });

                    foreach (var item in orderedGrouping)
                    {
                        groupingShape.Add(item);
                    }
                    container.Add(groupingShape);
                }

                htmlContentBuilder.AppendHtml(await DisplayAsync.ShapeExecuteAsync(container));
            }
            else if (groupings.Count == 1)
            {
                // Evaluate for cards.
                var cardGrouping = (GroupingViewModel)await ShapeFactory.CreateAsync<GroupingViewModel>("CardGrouping", m =>
                {
                    m.Identifier = identifier;
                    m.Grouping = groupings[0];
                });
                htmlContentBuilder.AppendHtml(await DisplayAsync.ShapeExecuteAsync(cardGrouping));
            }

            return htmlContentBuilder;
        }

        [Shape]
        public async Task<IHtmlContent> CardGrouping(IDisplayHelper DisplayAsync, GroupingViewModel Shape, IShapeFactory ShapeFactory)
        {
            var htmlContentBuilder = new HtmlContentBuilder();

            var groupings = Shape.Grouping.GroupBy(x =>
            {
                // By convention all placement delimiters default to the name 'Content' when not specified during placement.
                var key = x.Metadata.Card;
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
                var orderedGroupings = groupings.OrderBy(grouping =>
                {
                    var firstGroupWithModifier = grouping.FirstOrDefault(group =>
                    {
                        var key = group.Metadata.Card;
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
                        var key = firstGroupWithModifier.Metadata.Card;
                        var modifierIndex = key.IndexOf(';');
                        return new PositionalGrouping(key.Substring(modifierIndex));
                    }

                    return new PositionalGrouping();
                }, FlatPositionComparer.Instance).ToList();


                var container = (GroupViewModel)await ShapeFactory.CreateAsync<GroupViewModel>("CardContainer", m =>
                {
                    m.Identifier = Shape.Identifier;
                });
                
                foreach (var orderedGrouping in orderedGroupings)
                {
                    var groupingShape = (GroupingViewModel)await ShapeFactory.CreateAsync<GroupingViewModel>("Card", m =>
                    {
                        m.Identifier = Shape.Identifier;
                        m.Grouping = orderedGrouping;
                    });

                    foreach (var item in orderedGrouping)
                    {
                        groupingShape.Add(item);
                    }
                    container.Add(groupingShape);
                }

                htmlContentBuilder.AppendHtml(await DisplayAsync.ShapeExecuteAsync(container));
            }
            else
            {
                // Evaluate for columns.
                var groupingShape = (GroupingViewModel)await ShapeFactory.CreateAsync<GroupingViewModel>("ColumnGrouping", m =>
                {
                    m.Identifier = Shape.Identifier;
                    m.Grouping = Shape.Grouping;
                });

                htmlContentBuilder.AppendHtml(await DisplayAsync.ShapeExecuteAsync(groupingShape));
            }

            return htmlContentBuilder;
        }


        [Shape]
        public async Task<IHtmlContent> ColumnGrouping(IDisplayHelper DisplayAsync, GroupingViewModel Shape, IShapeFactory ShapeFactory)
        {
            var htmlContentBuilder = new HtmlContentBuilder();

            var groupings = Shape.Grouping.GroupBy(x =>
            {
                // By convention all placement delimiters default to the name 'Content' when not specified during placement.
                var key = x.Metadata.Column;
                if (String.IsNullOrEmpty(key))
                {
                    key = "Content";
                }

                // Remove column modifier.
                var modifierIndex = key.IndexOf('_');
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

                var orderedGroupings = groupings.OrderBy(grouping =>
                {
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

                var container = (GroupViewModel)await ShapeFactory.CreateAsync<GroupViewModel>("ColumnContainer", m =>
                {
                    m.Identifier = Shape.Identifier;
                });

                foreach (var orderedGrouping in orderedGroupings)
                {
                    var groupingShape = (GroupingViewModel)await ShapeFactory.CreateAsync<GroupingViewModel>("Column", m =>
                    {
                        m.Identifier = Shape.Identifier;
                        m.Grouping = orderedGrouping;
                    });

                    groupingShape.Classes.Add("ta-col-grouping");
                    groupingShape.Classes.Add("column-" + orderedGrouping.Key.HtmlClassify());

                    // To adjust this breakpoint apply a modifier of lg-3 to every column.
                    var columnClasses = "col-12 col-md";
                    if (columnModifiers.TryGetValue(orderedGrouping.Key, out var columnModifier))
                    {
                        // When the modifier also has a - assume it is providing a breakpointed class.
                        if (columnModifier.IndexOf('-') != -1)
                        {
                            columnClasses = "col-12 col-" + columnModifier;
                        }
                        else // Otherwise assume a default md breakpoint.
                        {
                            columnClasses = "col-12 col-md-" + columnModifier;
                        }
                    }

                    groupingShape.Classes.Add(columnClasses);

                    foreach (var item in orderedGrouping)
                    {
                        groupingShape.Add(item);
                    }
                    container.Add(groupingShape);
                }

                htmlContentBuilder.AppendHtml(await DisplayAsync.ShapeExecuteAsync(container));
            }
            else
            {
                // When nothing is grouped in a column, the grouping is rendered directly.
                foreach (var item in Shape.Grouping)
                {
                    htmlContentBuilder.AppendHtml(await DisplayAsync.ShapeExecuteAsync(item));
                }
            }

            return htmlContentBuilder;
        }
        private static Dictionary<string, string> GetColumnPositions(IList<IGrouping<string, IShape>> groupings)
        {
            var positionModifiers = new Dictionary<string, string>();
            foreach (var grouping in groupings)
            {
                var firstGroupWithModifier = FirstGroupingWithModifierOrDefault(grouping, ';');
                if (firstGroupWithModifier != null)
                {
                    var key = (string)firstGroupWithModifier.Metadata.Column;
                    var columnModifierIndex = key.IndexOf('_');
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

        private static Dictionary<string, string> GetColumnModifiers(IList<IGrouping<string, IShape>> groupings)
        {
            var columnModifiers = new Dictionary<string, string>();
            foreach (var grouping in groupings)
            {
                var firstGroupWithModifier = FirstGroupingWithModifierOrDefault(grouping, '_');
                if (firstGroupWithModifier != null)
                {
                    var key = (string)firstGroupWithModifier.Metadata.Column;
                    var posModifierIndex = key.IndexOf(';');
                    if (posModifierIndex != -1)
                    {
                        var colModifierIndex = key.IndexOf('_');
                        // Column;5.1_9
                        if (colModifierIndex > posModifierIndex)
                        {
                            columnModifiers.Add(key.Substring(0, posModifierIndex), key.Substring(colModifierIndex + 1));
                        }
                        else // Column_9;5.1
                        {
                            var length = posModifierIndex - colModifierIndex;
                            columnModifiers.Add(key.Substring(0, colModifierIndex), key.Substring(colModifierIndex + 1, length - 1));
                        }
                    }
                    else
                    {
                        var columnModifierIndex = key.IndexOf('_');
                        columnModifiers.Add(key.Substring(0, columnModifierIndex), key.Substring(columnModifierIndex + 1));
                    }
                }
            }

            return columnModifiers;
        }

        private static dynamic FirstGroupingWithModifierOrDefault(IGrouping<string, IShape> grouping, char modifier)
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
