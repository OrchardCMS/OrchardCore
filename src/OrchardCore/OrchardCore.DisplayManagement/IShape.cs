using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.DisplayManagement;

/// <summary>
/// Interface present on dynamic shapes.
/// May be used to access attributes in a strongly typed fashion.
/// Note: Anything on this interface is a reserved word for the purpose of shape properties.
/// </summary>
public interface IShape
{
    ShapeMetadata Metadata { get; }
    string Id { get; set; }
    string TagName { get; set; }
    IList<string> Classes { get; }
    IDictionary<string, string> Attributes { get; }
    IDictionary<string, object> Properties { get; }
    IReadOnlyList<IPositioned> Items { get; }
    ValueTask<IShape> AddAsync(object item, string position);
}

public static class IShapeExtensions
{
    /// <summary>
    /// Determines whether the specified shape is <see langword="null"/> or a deferred empty zone.
    /// </summary>
    /// <param name="shape">The shape to inspect.</param>
    /// <returns><see langword="true"/> if the shape is <see langword="null"/> or a <see cref="ZoneOnDemand"/>; otherwise <see langword="false"/>.</returns>
    public static bool IsNullOrEmpty(this IShape shape) => shape == null || shape is ZoneOnDemand;

    /// <summary>
    /// Determines whether the specified shape contains any items.
    /// </summary>
    /// <param name="shape">The shape to inspect.</param>
    /// <returns><see langword="true"/> if the shape contains one or more items; otherwise <see langword="false"/>.</returns>
    public static bool HasItems(this IShape shape)
    {
        if (shape is Shape s)
        {
            return s.HasItems;
        }

        return shape.Items is not null && shape.Items.Count > 0;
    }

    /// <summary>
    /// Determines whether the specified shape contains items in any zone except the excluded ones.
    /// </summary>
    /// <param name="shape">The shape to inspect.</param>
    /// <param name="excludedZones">The zone names to ignore during the check.</param>
    /// <returns><see langword="true"/> if a non-excluded zone contains items; otherwise <see langword="false"/>.</returns>
    public static bool HasItemsInAnyZone(this IShape shape, params string[] excludedZones)
    {
        if (shape is not IZoneHolding)
        {
            return false;
        }

        foreach (var zone in shape.Properties)
        {
            if (excludedZones is not null &&
                excludedZones.Any(z => string.Equals(zone.Key, z, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (zone.Value is IShape s && s.HasItems())
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Attempts to get a strongly typed property value from a shape.
    /// </summary>
    /// <typeparam name="T">The expected property type.</typeparam>
    /// <param name="shape">The shape containing the property.</param>
    /// <param name="key">The property name.</param>
    /// <param name="value">When this method returns, contains the property value if found and assignable to <typeparamref name="T"/>; otherwise the default value for <typeparamref name="T"/>.</param>
    /// <returns><see langword="true"/> if the property exists and is assignable to <typeparamref name="T"/>; otherwise <see langword="false"/>.</returns>
    public static bool TryGetProperty<T>(this IShape shape, string key, out T value)
    {
        if (shape.Properties != null && shape.Properties.TryGetValue(key, out var result))
        {
            if (result is T t)
            {
                value = t;
                return true;
            }
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Gets a property value from a shape.
    /// </summary>
    /// <param name="shape">The shape containing the property.</param>
    /// <param name="key">The property name.</param>
    /// <returns>The property value if found; otherwise <see langword="null"/>.</returns>
    public static object GetProperty(this IShape shape, string key)
    {
        return GetProperty(shape, key, (object)null);
    }

    /// <summary>
    /// Gets a strongly typed property value from a shape.
    /// </summary>
    /// <typeparam name="T">The expected property type.</typeparam>
    /// <param name="shape">The shape containing the property.</param>
    /// <param name="key">The property name.</param>
    /// <returns>The property value if found and assignable to <typeparamref name="T"/>; otherwise the default value for <typeparamref name="T"/>.</returns>
    public static T GetProperty<T>(this IShape shape, string key)
    {
        return GetProperty(shape, key, default(T));
    }

    /// <summary>
    /// Gets a strongly typed property value from a shape.
    /// </summary>
    /// <typeparam name="T">The expected property type.</typeparam>
    /// <param name="shape">The shape containing the property.</param>
    /// <param name="key">The property name.</param>
    /// <param name="value">The fallback value to return when the property is missing or not assignable to <typeparamref name="T"/>.</param>
    /// <returns>The property value if found and assignable to <typeparamref name="T"/>; otherwise <paramref name="value"/>.</returns>
    public static T GetProperty<T>(this IShape shape, string key, T value)
    {
        if (shape.Properties != null && shape.Properties.TryGetValue(key, out var result))
        {
            if (result is T t)
            {
                return t;
            }
        }

        return value;
    }

    /// <summary>
    /// Adds a sequence of items to the shape.
    /// </summary>
    /// <param name="shape">The target shape.</param>
    /// <param name="items">The items to add.</param>
    /// <param name="position">The position to use for each item.</param>
    /// <returns>The target shape.</returns>
    public static async ValueTask<IShape> AddRangeAsync(this IShape shape, IEnumerable<object> items, string position = null)
    {
        foreach (var item in items)
        {
            await shape.AddAsync(item, position);
        }

        return shape;
    }

    /// <summary>
    /// Adds an item to the shape using the default position.
    /// </summary>
    /// <param name="shape">The target shape.</param>
    /// <param name="item">The item to add.</param>
    /// <returns>The target shape.</returns>
    public static ValueTask<IShape> AddAsync(this IShape shape, object item)
    {
        return shape.AddAsync(item, "");
    }

    /// <summary>
    /// Creates a <see cref="TagBuilder"/> using the shape metadata and HTML attributes.
    /// </summary>
    /// <param name="shape">The shape to convert.</param>
    /// <param name="defaultTagName">The tag name to use when the shape does not specify one.</param>
    /// <returns>A configured <see cref="TagBuilder"/>.</returns>
    public static TagBuilder GetTagBuilder(this IShape shape, string defaultTagName = "span")
    {
        var tagName = defaultTagName;

        // We keep this for backward compatibility.
        if (shape.TryGetProperty("Tag", out string valueString))
        {
            tagName = valueString;
        }

        if (!string.IsNullOrEmpty(shape.TagName))
        {
            tagName = shape.TagName;
        }

        var tagBuilder = new TagBuilder(tagName);

        if (shape.Attributes?.Count > 0)
        {
            tagBuilder.MergeAttributes(shape.Attributes, false);
        }

        if (shape.Classes?.Count > 0)
        {
            // Faster than AddCssClass which will do twice as many concatenations as classes.
            tagBuilder.Attributes["class"] = string.Join(' ', shape.Classes);
        }

        if (!string.IsNullOrWhiteSpace(shape.Id))
        {
            tagBuilder.Attributes["id"] = shape.Id;
        }

        return tagBuilder;
    }

    /// <summary>
    /// Serializes a shape to a JSON object.
    /// </summary>
    /// <param name="shape">The shape to serialize.</param>
    /// <returns>A <see cref="JsonObject"/> representation of the shape.</returns>
    public static JsonObject ShapeToJson(this IShape shape) => new ShapeSerializer(shape).Serialize();
}
