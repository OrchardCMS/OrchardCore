using Microsoft.AspNetCore.Razor.TagHelpers;

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

        if (!output.Attributes.TryGetAttribute("cache-id", out _))
        {
            output.Attributes.Add("cache-id", "user-display-name");
        }

        if (output.Attributes.TryGetAttribute("cache-tag", out var cacheTag))
        {
            output.Attributes.SetAttribute("cache-tag", $"{Convert.ToString(cacheTag.Value)},user-display-name,user-display-name:{UserName}");
        }
        else
        {
            output.Attributes.Add("cache-tag", $"user-display-name,user-display-name:{UserName}");
        }

        if (output.Attributes.TryGetAttribute("cache-context", out var cacheContext))
        {
            output.Attributes.SetAttribute("cache-context", $"{Convert.ToString(cacheContext.Value)},username-{UserName}");
        }
        else
        {
            output.Attributes.Add("cache-context", $"username-{UserName}");
        }

        return base.ProcessAsync(tagHelperContext, output);
    }
}
