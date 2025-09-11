using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DisplayManagement.TagHelpers;

[HtmlTargetElement("user-display-name")]
[HtmlTargetElement("UserDisplayName")]
public class UserDisplayNameTagHelper : BaseShapeTagHelper
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
            if (!Properties.TryGetValue("Username", out var usernameObj))
            {
                output.SuppressOutput();

                return;
            }

            Username = usernameObj.ToString();
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

        var wrapperShape = await _shapeFactory.CreateAsync("UserDisplayName", Arguments.From(new { Username, Title, }));

        if (!string.IsNullOrWhiteSpace(DisplayType))
        {
            wrapperShape.Metadata.DisplayType = DisplayType;
        }

        tagHelperContext.Items[typeof(IShape)] = wrapperShape;

        if (!string.IsNullOrWhiteSpace(Cache))
        {
            var metadata = wrapperShape.Metadata;

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

        output.Content.SetHtmlContent(await _displayHelper.ShapeExecuteAsync(wrapperShape));

        output.TagName = null;
    }
}
