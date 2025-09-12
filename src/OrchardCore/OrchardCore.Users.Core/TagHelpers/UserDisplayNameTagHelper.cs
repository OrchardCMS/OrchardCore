using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.TagHelpers;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Users.TagHelpers;

[HtmlTargetElement("user-display-name")]
[HtmlTargetElement("UserDisplayName")]
public sealed class UserDisplayNameTagHelper : BaseShapeTagHelper
{
    public UserDisplayNameTagHelper(IShapeFactory shapeFactory, IDisplayHelper displayHelper)
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

    public override Task ProcessAsync(TagHelperContext tagHelperContext, TagHelperOutput output)
    {
        if (string.IsNullOrEmpty(Username))
        {
            if (!Properties.TryGetValue("Username", out var name))
            {
                output.SuppressOutput();

                return Task.CompletedTask;
            }

            Username = name.ToString();
        }

        Properties["Username"] = Username;

        if (!string.IsNullOrWhiteSpace(Title))
        {
            Properties["Title"] = Title;
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

        if (!string.IsNullOrWhiteSpace(DisplayType))
        {
            output.Attributes.Add("display-type", DisplayType);
        }

        return base.ProcessAsync(tagHelperContext, output);
    }

    protected override ValueTask ShapeBuildingAsync(IShape shape)
    {
        var displayType = shape.Metadata.DisplayType?.EncodeAlternateElement() ?? "Detail";

        var username = Username.EncodeAlternateElement();

        // UserDisplayName_[DisplayType]__[Username] e.g. UserDisplayName-johndoe.SummaryAdmin.cshtml
        shape.Metadata.Alternates.Add("UserDisplayName_" + displayType + "__" + username);

        // UserDisplayName_[DisplayType] e.g. UserDisplayName.SummaryAdmin.cshtml
        shape.Metadata.Alternates.Add("UserDisplayName_" + displayType);

        return ValueTask.CompletedTask;
    }
}
