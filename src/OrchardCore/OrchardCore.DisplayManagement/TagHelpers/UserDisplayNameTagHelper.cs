using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.DisplayManagement.TagHelpers;

[HtmlTargetElement("user-display-name")]
[HtmlTargetElement("UserDisplayName")]
public sealed class UserDisplayNameTagHelper : BaseShapeTagHelper
{
    public UserDisplayNameTagHelper(IShapeFactory shapeFactory, IDisplayHelper displayHelper)
        : base(shapeFactory, displayHelper)
    {
        Type = "UserDisplayName";
    }

    [HtmlAttributeName("user-name")]
    public string UserName { get; set; }

    [HtmlAttributeName("title")]
    public string Title { get; set; }

    [HtmlAttributeName("display-type")]
    public string DisplayType { get; set; }

    public override Task ProcessAsync(TagHelperContext tagHelperContext, TagHelperOutput output)
    {
        if (string.IsNullOrEmpty(UserName))
        {
            if (!Properties.TryGetValue(nameof(UserName), out var name))
            {
                output.SuppressOutput();

                return Task.CompletedTask;
            }

            UserName = name.ToString();
        }

        Properties[nameof(UserName)] = UserName;

        if (!string.IsNullOrWhiteSpace(Title))
        {
            Properties[nameof(Title)] = Title;
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
            output.Attributes.Add("cache-tag", $"user-display-name,user-display-name:{UserName}");
        }

        if (!string.IsNullOrWhiteSpace(DisplayType))
        {
            output.Attributes.Add("display-type", DisplayType);
        }

        return base.ProcessAsync(tagHelperContext, output);
    }

    protected override ValueTask ShapeBuildingAsync(IShape shape)
    {
        var displayType = shape.Metadata.DisplayType?.EncodeAlternateElement() ?? "Detail";

        var userName = UserName.EncodeAlternateElement();

        // UserDisplayName_[DisplayType]__[UserName] e.g. UserDisplayName-johndoe.SummaryAdmin.cshtml
        shape.Metadata.Alternates.Add("UserDisplayName_" + displayType + "__" + userName);

        // UserDisplayName_[DisplayType] e.g. UserDisplayName.SummaryAdmin.cshtml
        shape.Metadata.Alternates.Add("UserDisplayName_" + displayType);

        return ValueTask.CompletedTask;
    }
}
