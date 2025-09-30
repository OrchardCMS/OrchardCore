using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.TagHelpers;

public abstract class BaseShapeTagHelper : TagHelper
{
    protected const string PropertyDictionaryName = "prop-all";
    protected const string PropertyPrefix = "prop-";

    private static readonly HashSet<string> _internalProperties =
    [
        "id",
        "alternate",
        "wrapper",
        "cache-id",
        "cache-context",
        "cache-tag",
        "cache-fixed-duration",
        "cache-sliding-duration"
    ];

    private static readonly char[] _separators = [',', ' '];

    protected IShapeFactory _shapeFactory;
    protected IDisplayHelper _displayHelper;

    public string Type { get; set; }

    // The following properties are declared as internal to prevent any attribute with a
    // matching name from being automatically bound and removed from the output attributes,
    // and then not added to the properties of the shape we are building.

    internal string Cache { get; set; }
    internal TimeSpan? FixedDuration { get; set; }
    internal TimeSpan? SlidingDuration { get; set; }
    internal string Context { get; set; }
    internal string Tag { get; set; }

    protected BaseShapeTagHelper(IShapeFactory shapeFactory, IDisplayHelper displayHelper)
    {
        _shapeFactory = shapeFactory;
        _displayHelper = displayHelper;
    }

    /// <summary>
    /// Additional properties for the shape.
    /// </summary>
    [HtmlAttributeName(PropertyDictionaryName, DictionaryAttributePrefix = PropertyPrefix)]
    public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

    public override async Task ProcessAsync(TagHelperContext tagHelperContext, TagHelperOutput output)
    {
        var properties = new Dictionary<string, object>();

        // These prefixed properties are bound with their original type and not converted as IHtmlContent
        foreach (var property in Properties)
        {
            var normalizedName = property.Key.ToPascalCaseDash();
            properties.Add(normalizedName, property.Value);
        }

        // Extract all other attributes from the tag helper, which are passed as IHtmlContent
        foreach (var pair in output.Attributes)
        {
            // Check it's not a reserved property name
            if (!_internalProperties.Contains(pair.Name))
            {
                var normalizedName = pair.Name.ToPascalCaseDash();

                if (!properties.ContainsKey(normalizedName))
                {
                    properties.Add(normalizedName, pair.Value.ToString());
                }
            }
        }

        if (string.IsNullOrWhiteSpace(Type))
        {
            Type = output.TagName;
        }

        if (string.IsNullOrWhiteSpace(Cache) && output.Attributes.TryGetAttribute("cache-id", out var cacheId))
        {
            Cache = Convert.ToString(cacheId);
        }

        if (string.IsNullOrWhiteSpace(Context) && output.Attributes.TryGetAttribute("cache-context", out var cacheContext))
        {
            Context = Convert.ToString(cacheContext);
        }

        if (string.IsNullOrWhiteSpace(Tag) && output.Attributes.TryGetAttribute("cache-tag", out var cacheTag))
        {
            Tag = Convert.ToString(cacheTag);
        }

        if (!FixedDuration.HasValue && output.Attributes.TryGetAttribute("cache-fixed-duration", out var cashDuration))
        {
            TimeSpan timespan;
            if (TimeSpan.TryParse(Convert.ToString(cashDuration), out timespan))
            {
                FixedDuration = timespan;
            }
        }

        if (!SlidingDuration.HasValue && output.Attributes.TryGetAttribute("cache-sliding-duration", out var slidingDuration))
        {
            TimeSpan timespan;
            if (TimeSpan.TryParse(Convert.ToString(slidingDuration), out timespan))
            {
                SlidingDuration = timespan;
            }
        }

        var shape = await _shapeFactory.CreateAsync(Type, Arguments.From(properties));

        if (output.Attributes.TryGetAttribute("id", out var id))
        {
            shape.Id = Convert.ToString(id);
        }

        if (output.Attributes.TryGetAttribute("alternate", out var alternate))
        {
            shape.Metadata.Alternates.Add(Convert.ToString(alternate));
        }

        if (output.Attributes.TryGetAttribute("wrapper", out var wrapper))
        {
            shape.Metadata.Wrappers.Add(Convert.ToString(wrapper));
        }

        if (output.Attributes.TryGetAttribute("display-type", out var displayType))
        {
            shape.Metadata.DisplayType = Convert.ToString(displayType.Value);
        }

        await ShapeBuildingAsync(shape);

        tagHelperContext.Items[typeof(IShape)] = shape;

        if (!string.IsNullOrWhiteSpace(Cache))
        {
            var metadata = shape.Metadata;

            metadata.Cache(Cache);

            if (FixedDuration.HasValue)
            {
                metadata.Cache().WithExpiryAfter(FixedDuration.Value);
            }

            if (SlidingDuration.HasValue)
            {
                metadata.Cache().WithExpirySliding(SlidingDuration.Value);
            }

            if (!string.IsNullOrWhiteSpace(Context))
            {
                var contexts = Context.Split(_separators, StringSplitOptions.RemoveEmptyEntries);
                metadata.Cache().AddContext(contexts);
            }

            if (!string.IsNullOrWhiteSpace(Tag))
            {
                var tags = Tag.Split(_separators, StringSplitOptions.RemoveEmptyEntries);
                metadata.Cache().AddTag(tags);
            }
        }

        await output.GetChildContentAsync();

        output.Content.SetHtmlContent(await _displayHelper.ShapeExecuteAsync(shape));

        // We don't want any encapsulating tag around the shape
        output.TagName = null;
    }

    protected virtual ValueTask ShapeBuildingAsync(IShape shape)
        => ValueTask.CompletedTask;
}
