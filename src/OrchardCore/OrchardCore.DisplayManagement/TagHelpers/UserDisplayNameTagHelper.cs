using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.DisplayManagement.TagHelpers;

[HtmlTargetElement("user-display-name")]
[HtmlTargetElement("UserDisplayName")]
public sealed class UserDisplayNameTagHelper : BaseShapeTagHelper
{
    private static readonly char[] _separators = [',', ' '];

    public UserDisplayNameTagHelper(
        IShapeFactory shapeFactory,
        IDisplayHelper displayHelper)
    : base(shapeFactory, displayHelper)
    {
        Type = "UserDisplayName";
    }

    [HtmlAttributeName("username")]
    public string Username { get; set; }

    [HtmlAttributeName("title")]
    public string Title { get; set; }

    [HtmlAttributeName("display-type")]
    public string DisplayType { get; set; }

    public override async Task ProcessAsync(TagHelperContext tagHelperContext, TagHelperOutput output)
    {
        if (string.IsNullOrEmpty(Username))
        {
            if (!Properties.TryGetValue("Username", out var name))
            {
                output.SuppressOutput();

                return;
            }

            Username = name.ToString();
        }

        if (!output.Attributes.TryGetAttribute("cache-id", out _))
        {
            output.Attributes.Add("cache-id", "user-display-name");
        }

        if (!output.Attributes.TryGetAttribute("cache-context", out _))
        {
            output.Attributes.Add("cache-context", "user");
        }

        if (!output.Attributes.TryGetAttribute("cache-tag", out _))
        {
            output.Attributes.Add("cache-tag", $"user-display-name,user-display-name:{Username}");
        }

        var shape = await _shapeFactory.CreateAsync("UserDisplayName", Arguments.From(new { Username, Title, }));

        if (!string.IsNullOrWhiteSpace(DisplayType))
        {
            shape.Metadata.DisplayType = DisplayType;
        }

        var displayType = shape.Metadata.DisplayType?.EncodeAlternateElement() ?? "Detail";

        var username = Username.EncodeAlternateElement();

        // UserDisplayName_[DisplayType]__[Username] e.g. UserDisplayName-johndoe.SummaryAdmin.cshtml
        shape.Metadata.Alternates.Add("UserDisplayName_" + displayType + "__" + username);

        // UserDisplayName_[DisplayType] e.g. UserDisplayName.SummaryAdmin.cshtml
        shape.Metadata.Alternates.Add("UserDisplayName_" + displayType);

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

        output.TagName = null;
    }
}
