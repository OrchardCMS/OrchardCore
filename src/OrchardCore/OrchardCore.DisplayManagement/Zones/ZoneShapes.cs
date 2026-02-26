using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.ViewModels;
using OrchardCore.Modules;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.Zones;

[Feature(Application.DefaultFeatureId)]
public class ZoneShapes : IShapeAttributeProvider
{
    // By convention all placement delimiters default to the name 'Content' when not specified during placement.
    private const string ContentKey = "Content";

    [Shape]
#pragma warning disable CA1822 // Mark members as static
    public async Task<IHtmlContent> Zone(IDisplayHelper DisplayAsync, IEnumerable<object> Shape)
#pragma warning restore CA1822 // Mark members as static
    {
        var htmlContentBuilder = new HtmlContentBuilder();
        foreach (var item in Shape)
        {
            htmlContentBuilder.AppendHtml(await DisplayAsync.ShapeExecuteAsync((IShape)item));
        }

        return htmlContentBuilder;
    }

    [Shape]
#pragma warning disable CA1822 // Mark members as static
    public async Task<IHtmlContent> ContentZone(IDisplayHelper DisplayAsync, dynamic Shape, IShapeFactory ShapeFactory)
#pragma warning restore CA1822 // Mark members as static
    {
        var htmlContentBuilder = new HtmlContentBuilder();

        // This maybe a collection of IShape, IHtmlContent, or plain object.
        var shapes = ((IEnumerable<object>)Shape);

        // Evaluate shapes for grouping metadata, when it is not an IShape it cannot be grouped.
        var isGrouped = shapes.Any(x => x is IShape s &&
            (s.Metadata.TabGrouping.HasValue ||
            s.Metadata.CardGrouping.HasValue ||
            s.Metadata.ColumnGrouping.HasValue));

        // When there is no grouping metadata on any shapes just render the Zone.
        if (!isGrouped)
        {
            foreach (var item in shapes)
            {
                htmlContentBuilder.AppendHtml(await DisplayAsync.ShapeExecuteAsync((IShape)item));
            }

            return htmlContentBuilder;
        }

        string identifier = Shape.Identifier ?? string.Empty;

        var groupings = shapes.ToLookup(x =>
        {
            if (x is IShape s)
            {
                var tabGrouping = s.Metadata.TabGrouping;
                if (!tabGrouping.HasValue)
                {
                    return ContentKey;
                }

                return tabGrouping.Name;
            }

            return ContentKey;
        });

        // Process Tabs first, then Cards, then Columns.
        if (groupings.Count > 1)
        {
            var orderedGroupings = groupings.OrderBy(grouping =>
            {
                var firstGroupWithModifier = grouping.FirstOrDefault(group =>
                {
                    if (group is IShape s && !string.IsNullOrEmpty(s.Metadata.TabGrouping.Position))
                    {
                        return true;
                    }

                    return false;
                });

                if (firstGroupWithModifier is IShape shape)
                {
                    return new PositionalGrouping { Position = shape.Metadata.TabGrouping.Position };
                }

                return new PositionalGrouping(null);
            }, FlatPositionComparer.Instance).ToArray();

            var container = (GroupingsViewModel)await ShapeFactory.CreateAsync<GroupingsViewModel>("TabContainer", m =>
            {
                m.Identifier = identifier;
                m.Groupings = orderedGroupings;
            });

            container.Classes.Add("accordion");

            foreach (var orderedGrouping in orderedGroupings)
            {
                var groupingShape = (GroupingViewModel)await ShapeFactory.CreateAsync<GroupingViewModel>("Tab", m =>
                {
                    m.Identifier = identifier;
                    m.Grouping = orderedGrouping;
                });

                foreach (var item in orderedGrouping)
                {
                    await groupingShape.AddAsync(item);
                }

                await container.AddAsync(groupingShape);
            }

            htmlContentBuilder.AppendHtml(await DisplayAsync.ShapeExecuteAsync(container));
        }
        else if (groupings.Count == 1)
        {
            // Evaluate for cards.
            var cardGrouping = (GroupingViewModel)await ShapeFactory.CreateAsync<GroupingViewModel>("CardGrouping", m =>
            {
                m.Identifier = identifier;
                m.Grouping = groupings.ElementAt(0);
            });

            htmlContentBuilder.AppendHtml(await DisplayAsync.ShapeExecuteAsync(cardGrouping));
        }

        return htmlContentBuilder;
    }

    [Shape]
#pragma warning disable CA1822 // Mark members as static
    public async Task<IHtmlContent> CardGrouping(IDisplayHelper DisplayAsync, GroupingViewModel Shape, IShapeFactory ShapeFactory)
#pragma warning restore CA1822 // Mark members as static
    {
        var htmlContentBuilder = new HtmlContentBuilder();

        var groupings = Shape.Grouping.ToLookup(x =>
        {
            if (x is IShape s)
            {
                var cardGrouping = s.Metadata.CardGrouping;
                if (!cardGrouping.HasValue)
                {
                    return ContentKey;
                }

                return cardGrouping.Name;
            }

            return ContentKey;
        });

        if (groupings.Count > 1)
        {
            var orderedGroupings = groupings.OrderBy(grouping =>
            {
                var firstGroupWithModifier = grouping.FirstOrDefault(group =>
                {
                    if (group is IShape s && !string.IsNullOrEmpty(s.Metadata.CardGrouping.Position))
                    {
                        return true;
                    }

                    return false;
                });

                if (firstGroupWithModifier is IShape shape)
                {
                    return new PositionalGrouping { Position = shape.Metadata.CardGrouping.Position };
                }

                return new PositionalGrouping();
            }, FlatPositionComparer.Instance);

            var container = (GroupViewModel)await ShapeFactory.CreateAsync<GroupViewModel>("CardContainer", m =>
            {
                m.Identifier = Shape.Identifier;
            });

            container.Classes.Add("accordion");

            foreach (var orderedGrouping in orderedGroupings)
            {
                var groupingShape = (GroupingViewModel)await ShapeFactory.CreateAsync<GroupingViewModel>("Card", m =>
                {
                    m.Identifier = Shape.Identifier;
                    m.Grouping = orderedGrouping;
                });

                foreach (var item in orderedGrouping)
                {
                    await groupingShape.AddAsync(item);
                }

                await container.AddAsync(groupingShape);
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
#pragma warning disable CA1822 // Mark members as static
    public async Task<IHtmlContent> ColumnGrouping(IDisplayHelper DisplayAsync, GroupingViewModel Shape, IShapeFactory ShapeFactory)
#pragma warning restore CA1822 // Mark members as static
    {
        var htmlContentBuilder = new HtmlContentBuilder();

        var groupings = Shape.Grouping.ToLookup(x =>
        {
            if (x is IShape s)
            {
                var columnGrouping = s.Metadata.ColumnGrouping;
                if (!columnGrouping.HasValue)
                {
                    return ContentKey;
                }

                return columnGrouping.Name;
            }

            return ContentKey;
        });

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
            }, FlatPositionComparer.Instance);

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
                    if (columnModifier.Contains('-'))
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
                    await groupingShape.AddAsync(item);
                }
                await container.AddAsync(groupingShape);
            }

            htmlContentBuilder.AppendHtml(await DisplayAsync.ShapeExecuteAsync(container));
        }
        else
        {
            // When nothing is grouped in a column, the grouping is rendered directly.
            foreach (var item in Shape.Grouping)
            {
                htmlContentBuilder.AppendHtml(await DisplayAsync.ShapeExecuteAsync((IShape)item));
            }
        }

        return htmlContentBuilder;
    }

    private static Dictionary<string, string> GetColumnPositions(ILookup<string, object> groupings)
    {
        var positionModifiers = new Dictionary<string, string>();
        foreach (var grouping in groupings)
        {
            var firstGroupWithModifier = FirstGroupingWithPositionOrDefault(grouping);
            if (firstGroupWithModifier is IShape shape)
            {
                var columnGrouping = shape.Metadata.ColumnGrouping;
                positionModifiers.Add(columnGrouping.Name, columnGrouping.Position);
            }
        }

        return positionModifiers;
    }

    private static Dictionary<string, string> GetColumnModifiers(IEnumerable<IGrouping<string, object>> groupings)
    {
        var columnModifiers = new Dictionary<string, string>();
        foreach (var grouping in groupings)
        {
            var firstGroupWithModifier = FirstGroupingWithWidthOrDefault(grouping);
            if (firstGroupWithModifier is IShape shape)
            {
                var columnGrouping = shape.Metadata.ColumnGrouping;
                columnModifiers.Add(columnGrouping.Name, columnGrouping.Width);
            }
        }

        return columnModifiers;
    }

    private static object FirstGroupingWithPositionOrDefault(IGrouping<string, object> grouping)
    {
        return grouping.FirstOrDefault(group =>
        {
            if (group is IShape s && !string.IsNullOrEmpty(s.Metadata.ColumnGrouping.Position))
            {
                return true;
            }

            return false;
        });
    }

    private static object FirstGroupingWithWidthOrDefault(IGrouping<string, object> grouping)
    {
        return grouping.FirstOrDefault(group =>
        {
            if (group is IShape s && !string.IsNullOrEmpty(s.Metadata.ColumnGrouping.Width))
            {
                return true;
            }

            return false;
        });
    }
}

internal sealed class PositionalGrouping : IPositioned
{
    public PositionalGrouping()
    {
    }

    public PositionalGrouping(string key)
    {
        if (!string.IsNullOrEmpty(key))
        {
            var modifierIndex = key.IndexOf(';');
            if (modifierIndex != -1)
            {
                Position = key[(modifierIndex + 1)..];
            }
        }
    }
    public string Position { get; set; }
}
