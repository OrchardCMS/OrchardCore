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
        var shapes = (IEnumerable<object>)Shape;

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

        var identifier = Shape.Identifier ?? string.Empty;

        var groupings = shapes.ToLookup(shape =>
        {
            if (shape is IShape s)
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
                var firstGroupWithModifier = grouping.FirstOrDefault(group => group is IShape shape &&
                    !string.IsNullOrEmpty(shape.Metadata.TabGrouping.Position));

                if (firstGroupWithModifier is IShape shape)
                {
                    return shape.Metadata.TabGrouping.Position;
                }

                return null;
            }, FlatPositionComparer.Instance).ToArray();

            var container = await ShapeFactory.CreateAsync<GroupingsViewModel>("TabContainer", m =>
            {
                m.Identifier = identifier;
                m.Groupings = orderedGroupings;
            });

            container.Classes.Add("accordion");

            foreach (var orderedGrouping in orderedGroupings)
            {
                var groupingShape = await ShapeFactory.CreateAsync<GroupingViewModel>("Tab", m =>
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
            var cardGrouping = await ShapeFactory.CreateAsync<GroupingViewModel>("CardGrouping", m =>
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
            var container = await ShapeFactory.CreateAsync<GroupViewModel>("CardContainer", m =>
            {
                m.Identifier = Shape.Identifier;
            });

            container.Classes.Add("accordion");

            var orderedGroupings = groupings.OrderBy(grouping =>
            {
                var firstGroupWithModifier = grouping.FirstOrDefault(group =>
                    group is IShape s && !string.IsNullOrEmpty(s.Metadata.CardGrouping.Position));

                if (firstGroupWithModifier is IShape shape)
                {
                    return shape.Metadata.CardGrouping.Position;
                }

                return null;
            }, FlatPositionComparer.Instance);

            foreach (var orderedGrouping in orderedGroupings)
            {
                var groupingShape = await ShapeFactory.CreateAsync<GroupingViewModel>("Card", m =>
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
            var groupingShape = await ShapeFactory.CreateAsync<GroupingViewModel>("ColumnGrouping", m =>
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

        var allItems = Shape.Grouping.ToList();
        var hasColumnItems = allItems.Any(x => x is IShape s && s.Metadata.ColumnGrouping.HasValue);

        if (hasColumnItems)
        {
            // Each column-specified item gets its own column wrapper within a row.
            // Non-column items render outside the row at their natural position.
            var beforeRow = new List<object>();
            var afterRow = new List<object>();
            var columnItems = new List<IShape>();
            var foundFirstColumnItem = false;

            foreach (var item in allItems)
            {
                if (item is IShape s && s.Metadata.ColumnGrouping.HasValue)
                {
                    foundFirstColumnItem = true;
                    columnItems.Add(s);
                }
                else
                {
                    if (!foundFirstColumnItem)
                    {
                        beforeRow.Add(item);
                    }
                    else
                    {
                        afterRow.Add(item);
                    }
                }
            }

            // Render non-column items that appear before the first column item.
            foreach (var item in beforeRow)
            {
                htmlContentBuilder.AppendHtml(await DisplayAsync.ShapeExecuteAsync((IShape)item));
            }

            // Sort column items by their column position.
            var sortedColumnItems = columnItems
                .OrderBy(s => s.Metadata.ColumnGrouping.Position, FlatPositionComparer.Instance);

            var container = (GroupViewModel)await ShapeFactory.CreateAsync<GroupViewModel>("ColumnContainer", m =>
            {
                m.Identifier = Shape.Identifier;
            });

            foreach (var columnItem in sortedColumnItems)
            {
                var columnGrouping = columnItem.Metadata.ColumnGrouping;

                var columnShape = (GroupViewModel)await ShapeFactory.CreateAsync<GroupViewModel>("Column", m =>
                {
                    m.Identifier = Shape.Identifier;
                });

                columnShape.Classes.Add("ta-col-grouping");
                columnShape.Classes.Add("column-" + columnGrouping.Name.HtmlClassify());

                // To adjust this breakpoint apply a modifier of lg-3 to every column.
                var columnClasses = "col-12 col-md";
                if (!string.IsNullOrEmpty(columnGrouping.Width))
                {
                    // When the modifier also has a - assume it is providing a breakpointed class.
                    if (columnGrouping.Width.Contains('-'))
                    {
                        columnClasses = "col-12 col-" + columnGrouping.Width;
                    }
                    else // Otherwise assume a default md breakpoint.
                    {
                        columnClasses = "col-12 col-md-" + columnGrouping.Width;
                    }
                }

                columnShape.Classes.Add(columnClasses);

                await columnShape.AddAsync(columnItem);
                await container.AddAsync(columnShape);
            }

            htmlContentBuilder.AppendHtml(await DisplayAsync.ShapeExecuteAsync(container));

            // Render non-column items that appear after the column row.
            foreach (var item in afterRow)
            {
                htmlContentBuilder.AppendHtml(await DisplayAsync.ShapeExecuteAsync((IShape)item));
            }
        }
        else
        {
            // When nothing is grouped in a column, the grouping is rendered directly.
            foreach (var item in allItems)
            {
                htmlContentBuilder.AppendHtml(await DisplayAsync.ShapeExecuteAsync((IShape)item));
            }
        }

        return htmlContentBuilder;
    }

}
